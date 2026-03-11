# Global Key Shorts

可以在Window的生命周期事件`SourceInitialized`或比其更晚的事件如`Loaded`中注册快捷键。关闭Window时(`Closed事件`)会自动注销快捷键。

```c#
// 注册快捷键
protected override void OnSourceInitialized(EventArgs e)
{
    base.OnSourceInitialized(e);
    var hotKeyModel = new HotKeyModel("Ctrl+Shift+A", true, true, false, false, Keys.A);
    this.GlobalHotkeyManager.RegisterGlobalHotKey(hotKeyModel, (model) =>
    {
        MessageBox.Show($"Hotkey pressed: {model.Name} ({model.Key})\nModifiers: {model.GetModifierKeys()}",
        "Global Hotkey Triggered", MessageBoxButton.OK, MessageBoxImage.Information);
    });
}
```

```c#
// 注销快捷键
GlobalHotkeyManager.UnregisterGlobalHotKey("Ctrl+Shift+A");
```

```c#
// 获取整个application成功注册的快捷键
IEnumerable<HotKeyModel> models = GlobalHotkeyManager.GetAllHotkeys();
```

```c#
// 获取在指定Window上注册的快捷键
IEnumerable<HotKeyModel> models = GlobalHotkeyManager.GetHotkeysOnWindow(this);
```

- 快捷键事件处理程序`action`在创建window的UI 线程上执行，应当妥善处理它可能抛出的异常，避免导致UI线程崩溃。
- 其他进程或当前进程已经注册的快捷键，重复注册会失败。
- 如果多个进程使用相同的快捷键，先启动的先占用，后来者失败。
- 一个进程只能注销本进程已经注册的快捷键，无法注销另外一个进程的快捷键。
- 开发者可以随时注销已经注册的快捷键，也可以不用关心，`快捷键会在窗口关闭时自动注销`。
- 一般是在主窗体的OnSourceInitialized注册快捷键，只要主窗体没被关闭，快捷键一直有效。

# BindingProxy

UserControl有依赖属性`IsDescriptionColumnVisible`,子控件`DataGrid`.

```c#
public bool IsDescriptionColumnVisible
{
    get { return (bool)GetValue(IsDescriptionColumnVisibleProperty); }
    set { SetValue(IsDescriptionColumnVisibleProperty, value); }
}
```

DataGrid的`Description Column`的可见性绑定到依赖属性`IsDescriptionColumnVisible`,

如，`Visibility="{Binding ElementName="userControl", Path=IsDescriptionColumnVisible, Converter={StaticResource Bool2VisibilityConverter}}"`

但是会发现并不起作用。打开Visual Studio的Output窗口可以看到错误信息`System.Windows.Data Error: 2 : Cannot find governing FrameworkElement or FrameworkContentElement for target element. BindingExpression:Path=IsDescriptionColumnVisible; DataItem=null; target element is 'DataGridTemplateColumn' (HashCode=53540541); target property is 'Visibility' (type 'Visibility')`

总之就是DataGridTemplateColumn不在UserControl的可视化树上，导致绑定无效。

使用BindingProxy可以解决问题。在资源中定义BindingProxy，BindingProxy的DataContext与资源宿主的DataContext相同。

```xml
<UserControl.Resources>
    <xp:BindingProxy x:Key="proxy" Data="{Binding ElementName=self}" />
</UserControl.Resources>

<DataGridTemplateColumn
    Header="Description"
    MinWidth="{Binding Source={StaticResource proxy}, Path=Data.DescColumnMinWidth}"
    MaxWidth="{Binding Source={StaticResource proxy}, Path=Data.DescColumnMaxWidth}"
    Visibility="{Binding Source={StaticResource proxy}, Path=Data.IsDescriptionColumnVisible, Converter={StaticResource Bool2VisibilityConverter}}">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Description}" />
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>

```
# RoutedEventTrigger

减少冒泡&隧道路由事件订阅次数的同时，还能将事件处理程序从XAML Code Behind 转移到 ViewModel，让代码更符合MVVM规范。

**Bad Code**

```xaml
<StackPanel
    x:Name="StackPanel"
    Margin="0,10,0,0"
    ButtonBase.Click="StackPanel_Click">
    <Button
        MinWidth="150"
        Margin="10"
        Padding="5"
        HorizontalAlignment="Center"
        Content="Move forward 10cm"
        Tag="10" />
    <Button
        MinWidth="150"
        Margin="10"
        Padding="5"
        HorizontalAlignment="Center"
        Content="Move forward 20cm"
        Tag="20" />
    <Button
        MinWidth="150"
        Margin="10"
        Padding="5"
        HorizontalAlignment="Center"
        Content="Move forward 30cm"
        Tag="30" />
</StackPanel>
```

**更符合MVVM的好代码**

```xml
<StackPanel x:Name="StackPanel" Margin="0,10,0,0">
    <i:Interaction.Triggers>
        <xp:RoutedEventTrigger RoutedEvent="{x:Static ButtonBase.ClickEvent}">
            <prism:InvokeCommandAction Command="{Binding MoveCommand}" TriggerParameterPath="OriginalSource.Tag" />
        </xp:RoutedEventTrigger>
    </i:Interaction.Triggers>
    <Button
        MinWidth="150"
        Margin="10"
        Padding="5"
        HorizontalAlignment="Center"
        Content="Move forward 10cm"
        Tag="10" />
    <Button
        MinWidth="150"
        Margin="10"
        Padding="5"
        HorizontalAlignment="Center"
        Content="Move forward 20cm"
        Tag="20" />
    <Button
        MinWidth="150"
        Margin="10"
        Padding="5"
        HorizontalAlignment="Center"
        Content="Move forward 30cm"
        Tag="30" />
</StackPanel>
```

# CallMethodAction

| Property                          | Description                                                  |
| --------------------------------- | ------------------------------------------------------------ |
| **MethodName**                    | 指定要调用的方法名称                                         |
| **Parameters**                    | 指定方法的实参                                               |
| **TargetObject**                  | 方法所属的实例。默认值是CallMethodAction关联的WPF元素。      |
| **PassTriggerArgsToMethod**       | 是否传递Trigger传递给CallMethodAction的Invoke(object args)的参数args作为Method的实参 |
| **TriggerArgsConverter**          | 对Trigger传递的参数args进行转换（PassTriggerArgsToMethod等于True时生效） |
| **TriggerArgsConverterParameter** | 指定TriggerArgsConverter的`object Convert(object value, Type targetType, object parameter, CultureInfo culture)`的parameter实参（PassTriggerArgsToMethod等于True时生效） |

- 方法的形参可以是Parameters指定的实参的基类

  如后续代码中，ViewModel的OnSubmit()的firstName的类型是object，但是Parameters传递的实参是string，但是程序运行正常。

  还有void ShowMouseDownTime(MouseEventArgs e)，View传递到ViewModel的实参类型是MouseButtonEventArgs，程序依然正常运行。

- Parameters指定实参的顺序和数量必须匹配方法的签名

- 方法签名必须是public的实例方法且无返回void



**View**

```xml
<StackPanel Orientation="Horizontal">
    <!--  提交姓名表单  （两个参数）-->
    <StackPanel
        Margin="5"
        HorizontalAlignment="Center"
        VerticalAlignment="Center">
        <StackPanel VerticalAlignment="Center" Orientation="Horizontal">
            <TextBlock Margin="0,0,20,0" Text="First Name:" />
            <TextBox Name="FirstNameTxt" Width="200" />
        </StackPanel>

        <StackPanel
            Margin="5"
            VerticalAlignment="Center"
            Orientation="Horizontal">
            <TextBlock Margin="0,0,16,0" Text="Last Name:" />
            <TextBox Name="LastNameTxt" Width="200" />
        </StackPanel>

        <Button Width="100" Content="Submit">
            <i:Interaction.Triggers>
                <i:EventTrigger EventName="Click">
                    <xp:CallMethodAction MethodName="OnSubmit" TargetObject="{Binding RelativeSource={RelativeSource AncestorType=Window}, Path=DataContext}">
                        <xp:Parameter Value="{Binding ElementName=FirstNameTxt, Path=Text}" />
                        <xp:Parameter Value="{Binding ElementName=LastNameTxt, Path=Text}" />
                    </xp:CallMethodAction>
                </i:EventTrigger>
            </i:Interaction.Triggers>
        </Button>
    </StackPanel>

    <!--  关闭窗体 （无参） -->
    <Button
        Margin="30,0,0,0"
        Padding="5"
        VerticalAlignment="Center"
        Content="Close Window">
        <i:Interaction.Triggers>
            <i:EventTrigger EventName="Click">
                <xp:CallMethodAction MethodName="Close" TargetObject="{Binding RelativeSource={RelativeSource AncestorType=Window}}" />
            </i:EventTrigger>
        </i:Interaction.Triggers>
    </Button>

    <!--  显示鼠标信息  （传递事件原始参数）-->
    <TextBlock
        Margin="50,0,0,0"
        VerticalAlignment="Center"
        Text="Show Mouse Down Time">
        <i:Interaction.Triggers>
            <i:EventTrigger EventName="MouseDown">
                <xp:CallMethodAction
                    MethodName="ShowMouseDownTime"
                    PassTriggerArgsToMethod="True"
                    TargetObject="{Binding RelativeSource={RelativeSource AncestorType=Window}, Path=DataContext}" />
            </i:EventTrigger>
        </i:Interaction.Triggers>
    </TextBlock>
</StackPanel>

```



**ViewModel**

```c#
internal class MainWindowViewModel
{
    public void OnSubmit(object firstName,string lastName)
    {
        MessageBox.Show($"Hello {firstName} {lastName}!");
    }

    public void ShowMouseDownTime(MouseEventArgs e)
    {
        MessageBox.Show($"Mouse down at {e.Timestamp} ms");
    }
}
```



# LambdaConverters



The library allows to create `IValueConverter`, `IMultiValueConverter`, `DataTemplateSelector`, and `ValidationRule` objects with the most convenient syntax available, ideally, using the lambda expressions.

## IValueConverters

First define your converters as static fields (or properties) in UserControl class:

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public static readonly IValueConverter VisibleIfTrue = ValueConverter.Create<bool, Visibility>(
        e => e.Value ? Visibility.Visible : Visibility.Collapsed);
}

```

You're done! Just reference the converters with the `x:Static` expressions from your XAML files:

```xml
<Button Visibility="{Binding model.IsAvailable, Converter={x:Static local:MainWindow.VisibleIfTrue}}" />
```

**Features**

- *strongly-typed* converters
- resource declaration not needed, just use the `x:Static` expressions
- separate class for each converter not needed anymore
- no redundant declarations: if you do not need the `ConvertBack` method, don't define it; otherwise, just put the second lambda expression
- full support for the remaining parameters of the `Convert` and `ConvertBack` methods: the `culture` and the `parameter` (also strongly-typed) are accessible as well
- if the conversion fails due to unexpected value types the optional [error strategy](Sources/LambdaConverters.Wpf/ConverterErrorStrategy.cs) can be specified

## IMultiValueConverter

```c#
public static readonly IMultiValueConverter PipeLineFlowRate = MultiValueConverter.Create<object[], double>(
    e =>
    {
        double flowRate = 0.0;
        bool isValve1Open = System.Convert.ToBoolean(e.Values[0]);
        if (isValve1Open)
        {
            flowRate += 1.0;
        }
        bool isValve2Open = System.Convert.ToBoolean(e.Values[1]);
        if (isValve2Open)
        {
            flowRate += 1.0;
        }
        return flowRate;
    });
```



## Lambda Data Template Selectors

The library also allows to create `DataTemplateSelector` objects in the same convenient way as value converters. In order to define a selector simply write a static field (or property) similar to this snippet:

```csharp
internal static class TemplateSelector
{
    public static DataTemplateSelector AlternatingText =
        LambdaConverters.TemplateSelector.Create<int>(
            e => e.Item % 2 == 0
                ? (DataTemplate) ((FrameworkElement) e.Container)?.FindResource("BlackWhite")
                : (DataTemplate) ((FrameworkElement) e.Container)?.FindResource("WhiteBlack"));
}
```
Use your Lambda DataTemplateSelectors by referencing it with the `x:Static` markup extention (assuming that `s` is the namespace definition for the `TemplateSelector` class):

```xml
<UserControl.Resources>
    <DataTemplate x:Key="BlackWhite">
        <TextBlock Text="{Binding}" Foreground="Black" Background="White" />
    </DataTemplate>
    <DataTemplate x:Key="WhiteBlack">
        <TextBlock Text="{Binding}" Foreground="White" Background="Black" />
    </DataTemplate>
</UserControl.Resources>
<DockPanel>
    <ListBox ItemsSource="{Binding IntNumbers}"
             ItemTemplateSelector="{x:Static s:TemplateSelector.AlternatingText}">
    </ListBox>
</DockPanel>
```

Tada! All even numbers from `IntNumbers` are displayed with black font and white background and the odd numbers get the inverse font and background colors.

**Features**

- *strongly-typed* Selectors
- resource declaration not needed, just use the `x:Static` expressions
- separate class for each selector not needed anymore
- full support for the remaining parameter `container`. For example, if you need to grab a `DataTemplate` from where the selector is use (see the example above).

## Lambda Validation Rules

Furthermore, you'll get Lambda ValidationRules on top. By now you know "the drill". First, define a `ValidationRule`object like this:

```csharp
public static class Rule
{
    public static ValidationRule IsNumericString =
        LambdaConverters.Validator.Create<string>(
            e => e.Value.All(char.IsDigit)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Text has non-digit characters!"));
}
```
And then reference your new rule in vour `View` (assuming that `r` is the namespace definition for the `Rule` class):
```xml
<TextBox>
    <TextBox.Text>
        <Binding Path="Text" UpdateSourceTrigger="PropertyChanged">
            <Binding.ValidationRules>
                <x:Static Member="r:Rule.IsNumericString"/>
            </Binding.ValidationRules>
        </Binding>
    </TextBox.Text>
</TextBox>
```
Now, you made sure that only strings which consists of digits are passed to your `ViewModel`.

**Features**

- *strongly-typed* rules
- resource declaration not needed, just use the `x:Static` expressions
- separate class for each rule not needed anymore
- full support for the remaining parameter `culture`

# OneSecond

