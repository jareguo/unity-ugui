Unity4.6新UI系统初探(uGUI)
==========

引言
=========
Unity终于在即将到来的**4.6**版本内集成了**所见即所得**的UI解决方案[(视频)](http://blogs.unity3d.com/2014/05/28/overview-of-the-new-ui-system/)。事实上从近几个版本开始，Unity就在为这套系统做技术扩展，以保证最终能实现较理想的UI系统。本文试图通过初步的介绍和试用，让读者对这套系统有大体的了解，以便更进一步评估这套UI系统好不好用，适合用在什么项目。为了避免坑挖太深，更进一步的试用和评估我将在《[用uGUI开发自定义Toggle Slider控件](https://github.com/jaredoc/unity-ugui/tree/master/toggle_demo)》中进行论述。为论述方便，下文将这套New UI System简称为**uGUI**，并且以**X-UI**指代现有第三方UI插件。

（测试只针对Unity 4.6.0 beta 10，正式版可能会有所出入。目前Unity没提供文档，本人半桶水，欢迎群众在微博或Issues里吐槽！）

Rect Transform
=========
![Rect Transform](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/overview/img/RectTransform.png)

Rect Transform继承自Transform，是uGUI相比X-UI最显著的区别[[注1](#why_transform)]。当你为Empty GameObject加入一个UI Component时，Transform会自动转换为Rect Transform。Rect Transform尽量整合了X-UI常见的**anchor**(相对父物体的锚点), **pivot**(中点), **stretch**(拉伸)等属性。值得一提的是，这里的**anchor**是Rect而非Vector2，因为它不仅用于偏移，而且用于缩放。点击Rect Transform上的准心图标，还能在弹出的**Anchor Presets**面板中对其进行快速设置。

![Anchor Presets](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/overview/img/AnchorPresets.png)

这个面板还是不够直观，我们可以把它看成一张表，上面四个图标用于设置列，左边四个图标用于设置行，也可以直接点击里面的16个图标同时设置行和列。强大的地方是，按住**shift**时能同时设置**pivot**，这时能发现控件虽然不动但position已经在改变。如果按住**alt**，则设置anchor的同时设置**position**。如果shift和alt同时按住，那么你就能同时设置anchor, pivot和position。这个操作方式比起X-UI，真的高明很多，对多分辨率适配很有帮助。

除此之外，Rect Transform还提供了Blueprint和Lock Rect选项，前者用于对旋转过的元素进行定位，后者据说明是能在设置anchor时保持位置不变，暂时没搞明白。

排序
=========
![SortHierarchy](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/overview/img/SortHierarchy.png)

uGUI可以直接在Hierarchy面板中上下拖拽来对渲染进行排序(支持程序控制)，越上面的UI会越先被渲染，相比X-UI的global depth排序，这样的拖拽设计很讨好用户。同时在结构上则和ex2D采用的local depth类似，这样GO只和同级其它GO进行排序，开发组件会很方便。需要注意的是，这里排序只是相对UI而言，其它3D物体还是按原先的次序渲染，并且UI总是渲染在3D物体上面。这就导致你不能像用ex2D那样直接将粒子系统插入到两个UI之间。

<a name="draw_call_problem"></a>这种无需填写depth值的排序方式，容易导致**没有手工做sprite packing的free版用户**遇到draw call增加。因为所有物体的depth都是自动设置的，Unity保证了每个物体的depth都是唯一的。这时假设你有一个**格子**控件，每个控件用到了两个Sprite，但你并没有把Sprite都拼到同一张贴图上。于是你每复制一个新的格子出来，draw call就会增加2个，因为Unity会以格子为单位依次绘制。pro用户由于有sprite packing机制，不用担心这个问题。（这种情况在ex2D里，是以默认提供"unordered"的渲染方式来解决的，这也是NGUI的默认做法。在这种情况下ex2D会优先以相同depth的相同Sprite为单位绘制，因此不论有多少个格子，draw call都是2个。除非你就是希望以格子为单位进行渲染[[注6](#why_ordered)]，那么你可以在ex2D里设置渲染方式为"ordered"，或者在NGUI里给每个格子设置不同的depth。

控件
=========
![UI Component](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/overview/img/UiComponent.png)

uGUI自带了以上控件，其中Image用于显示Sprite，Raw Image用于显示Texture，Image Mask和Rect Mask用于clipping。所有控件都是MonoBehaviour，可以直接从Inspector里拖到其它GameObject上。

#### Image ####

![Image](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/overview/img/Image.png)

uGUI用Image控件显示图片，图片就是一个Sprite，这意味着Pro用户不用再制作atlas了，相比X-UI是个大进步，Free用户一样可以手动做Packing。Image提供了**Simple**, **Sliced**, **Tiled**, **Filled**四种效果，和X-UI保持一致。

#### Button ####

![Button](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/overview/img/ButtonOverview.png)

uGUI里，Button控件由两个GameObject组成，一个包含Image, Button等Component，一个包含Text等Component。这样设计很组件化，唯一的问题是当用户想修改Button时，容易不小心选中Label或其它实体。

Button Component主要执行Transition和事件两个操作。
- **Transition**可选择改变颜色、更换贴图或自定义动画，使用起来简单方便，也能利用动画定义更丰富的表现。我会再写一篇文章演示Button的Transition。
- **事件**也是所见即所得的，在OnClick里面可以添加多个命令，命令可以选择对应的目标、操作和参数。用法简单，有需要也可以换程序控制。
 - 目标可以是任意Object，例如其它GameObject或者Project里的Asset
 - 操作可以是需要设置的参数或调用的方法
 - 参数分成Dynamic和Static，Dynamic能将控件的参数**单向绑定**到目标参数，Static则将目标参数设置成预设值。按钮没有Dynamic参数，Toggle, Slider等控件才有。

事件
=========

#### Event Trigger ####

![Event Trigger](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/overview/img/EventTrigger.png)

uGUI控件往往只提供一个**自带事件**，要响应更多**基本事件**的话，需要添加Event Trigger组件。Event Trigger包含以下事件：

- PointerEnter,	PointerExit,	PointerDown,	PointerUp,	PointerClick
- Move, Drag,	Drop,	Scroll
- KeyDown,	KeyUp,	Select,	Deselect

可以在Event Trigger中Add多个事件，每个事件都可以添加多个命令，用法和控件自带事件一致。

#### Graphic Raycaster ####

![Graphic Raycaster](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/overview/img/GraphicRaycaster.png)

每个Canvas都有一个Graphic Raycaster，用于获取用户选中的uGUI控件。多个Canvas之间通过设置Graphic Raycaster的priority来设置事件响应的先后次序。当Canvas采用World Space或Camera Space时，Graphic Raycaster的Block选项可以用来设置遮挡目标。

#### Event System ####

![Event System](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/overview/img/EventSystem.png)

创建uGUI控件后，Unity会同时创建一个[[注4](#eventSys)]叫EventSystem的GameObject，用于控制各类事件。可以看到Unity自带了两个Input Module，一个用于响应标准输入，一个用于响应触摸操作。Input Module封装了对Input模块的调用，根据用户操作触发各**Event Trigger**。理论上我们可以编写自己的Input Module，用来封装各种外部设备的输入，只要加入Event System所在的GameObject就行。

Event System组件则统一管理多个Input Module和**各种Raycaster**。它每一帧调用多个Input Module处理用户操作，也负责调用多个Raycaster用于获取用户点击的uGUI控件以及2D和3D物体。

性能
=========
2D渲染分两大类，一类是单纯的Sprite绘制，用于渲染场景、角色、粒子等，另一类是UI绘制。Unity将这两类需求划分成了**SpriteRenderer**和**uGUI**两部分，前者由**Transform** + **SpriteRenderer**实现，后者由**Rect Transform** + **CanvasRenderer** + **UI控件** + **Canvas[[注2](#why_canvas)]**实现，这样的两套相对独立的机制比起X-UI的UI控件继承自SpriteRenderer更为合理。因为在2D游戏里SpriteRenderer只需要关心最基本的面片渲染，注重效率，而UI注重各类变换、对齐、操作、动画，还常常需要Resize VBO。如果SpriteRenderer在设计上需要兼顾UI，就会像X-UI那样设计得太过复杂，在用户体验和性能上都很不好。

这里我们探讨一下uGUI的**渲染机制**，当我们渲染多个使用相同Sprite的控件时，并没发生dynamic batching，但是drawcall也没有上升。这就说明Unity在内部使用了专门的一套batching机制，把多个控件的VBO事先合并成了一个。也就是说CanvasRenderer不负责实际渲染，而是由Canvas批量渲染多个CanvasRenderer，这和部分X-UI采用的做法一致。这样单独batch的设计有可能使得性能比SpriteRenderer好，也可能导致性能更差。性能会更好的情况在ex2D里已经证实了，主要原因是这样能更好的**平衡CPU和GPU负载**，并且能做到**更优化的batching算法**。性能更差的情况，在去年旧版的NGUI测试时也遇到了，根本原因还是优化不到位导致的（不是贬低，不同工具的取舍和面向市场都不同）。而Unity的SpriteRenderer在手机上的渲染跑分是和ex2D持平的，CanvasRenderer又比SpriteRenderer快[[注3](#why_CanvasRenderer_better)]，因此uGUI的性能不用担心。由于目前没有Mac版本，我会在正式版发布后进行一次手机跑分测试。

小结
=========

uGUI功能完善，操作简洁，很接地气。可以说uGUI是相对X-UI的全面升级，整体架构更为严谨，实现更为清晰。依托4.5的Module Manager，uGUI以Package的形式提供，也能获得快速的升级[[注5](#package)]。作为ex2D v2.0开发者之一，我很看好它将来的发展，uGUI将在大多数场合取代X-UI。

初步感受：

#### 亮点 ####
- RectTransform
- Event/单向数据绑定
- 直接在Hierarchy中排序
- Pro用户可用Sprite的动态拼图，无需手工拼图

#### 不足 ####
- [无需填写depth值的排序方式，容易导致**没有手工做sprite packing的free版用户**遇到draw call增加。](#draw_call_problem)

#### 小遗憾 ####
- Anchor Presets面板还不够直观。
- 用户想修改Button时，很容易修改到Label。
- 当Hierarchy面板内的目标节点展开子节点后，无法将其它节点直接拖动到目标的正下方。

#### 小问题 ####
- Input组件对方向键的支持有问题。
- Game View dock到主窗口后，top定位有误，把toolbar的高度也算进去了。


附注
=========
1. <a name="why_transform"></a>我们在其它平台上开发类Entity-Component框架时，讨论过Unity为什么不在底层对transform做特殊处理，以避免插件作者手工缓存transform来优化query transform引起的开销，甚至是将transform直接整合进GameObject。原因是现在的transform是3D的，将来完全可能推出2D Transform。所以Unity在之前的版本里一直保留着transform的独立性。
2. <a name="why_canvas"></a>我不能完全肯定一定是Canvas，但通过Canvas和CanvasRenderer的接口来看，这个可能性很大。
3. <a name="why_CanvasRenderer_better"></a>基于更好的平衡CPU和GPU负载 + 更优化的batching算法，以Unity的实力CanvasRenderer超越SpriteRenderer问题不大。而且如果性能不会提升，uGUI只要像**2D Toolkit**那样给每个控件直接添加MeshRenderer，也就是说uGUI直接用已有的SpriteRenderer就好，不太可能加入新的CanvasRenderer性能反而更慢。
4. <a name="eventSys"></a>Unity允许多个Event System同时存在，但同一时刻只有一个能够生效。
5. <a name="package"></a>uGUI的控件、Event等模块以包的形式提供，位于程序目录下的*%UNITY%\Editor\Data\UnityExtensions\Unity\GUISystem\4.6.0*，Unity提供了两个运行时版本的DLL，分别用于创作和发布。区别主要是发布版不含一些Editor中才用得到的代码。由于DLL没办法通过预编译符号来进行条件编译，因此Unity使用这种方式进行权衡，用户发布时无需手工切换DLL版本，满足了闭源，又兼顾了执行效率。~~这样就甩开了第三方插件几条街，很多插件在这个问题上不是牺牲性能就是无奈开源。~~
6. <a name="why_ordered"></a>有时还是会需要以格子为单位渲染，例如当格子之间需要重合，这种需求在UI里不常见。

> [Jare](http://weibo.com/u/1751917933) @ [梦加网络](http://www.mechanist.co/cn/)

> 本文托管于 https://github.com/jareguo/unity-ugui/tree/master/overview 欢迎Fork、提Issue或转载
