用uGUI开发自定义Toggle Slider控件
==========

写完《[Unity4.6新UI系统初探](https://github.com/jaredoc/unity-ugui/tree/master/overview)》后，我模仿手机上的UI分别用**uGui**和**NGUI**做了一个仅用作演示的ToggleSlider，我认为这个小小的控件已能体现自定义控件的开发过程。由于手头上没有mac版，暂时未能真机测试，PC上的效果如下：

![uGui Toggle Slider](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/toggle_demo/img/uGuiToggleSlider.gif)

制作过程
----------

完整工程托管于[github](https://github.com/jaredoc/unity-ugui/tree/master/toggle_demo)，分为[uGui](https://github.com/jaredoc/unity-ugui/tree/master/toggle_demo/ugui_project)和[NGUI](https://github.com/jaredoc/unity-ugui/tree/master/toggle_demo/ngui_project)两个project。考虑到版权问题，工程里不含NGUI，同学们需自行将NGUI导进工程。NGUI需要Unity 4.5，uGui需要Unity 4.6。

### 功能点 ###

- 滑块可以拖动，从一边拖到另一边将改变控件值。
- 用户停止操作时，滑块如果居中，会自动滑向最近的一边。
- 点击滑块或整个控件，控件值将被改变，滑块自动滑向另一边。
- 控件值被其它脚本修改时，滑块自动滑向另一边。
- 滑块移动的过程中，如果值发生变化，滑块会以当前位置为起点滑回去。

下面以uGui为例简述制作方法，NGUI的方法也差不多，两者的区别可参考下文[[和NGUI对比](#compare_ngui)]。

### Hierarchy ###

![uGui Toggle Slider](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/toggle_demo/img/uGuiHierarchy.png)

上图是用uGui制作好的层级结构。其中，

- Canvas负责渲染UI。
  - Padding没什么用，只是画了一个边框。
    - **Toggle Slider**是控件的父物体。
      - **BackgroundAndMask**使用**ImageMask**组件作为SymbolOff的遮罩，同时渲染灰色底图。
        - **SymbolOff**是灰色的twitter小鸟，坐标受动画控制。
      - **Background_On**使用**Image**组件渲染蓝色高亮底图，Color.alpha受动画控制。
      - **BackgroundMaskOnly**使用**ImageMask**组件作为SymbolOn的遮罩，并不渲染。
        - **SymbolOn**是蓝色的twitter小鸟，坐标受动画控制。（不用Background_On作遮罩是因为蓝色底图的边缘是半透明的。）
      - **Handle**是正方形滑块，坐标**只**受动画控制。
    - Current Value是下面那个可选框，用于测试Toggle Slider。
- EventSystem可参考上篇文章。

### Toggle Slider GO ###

![uGui Toggle Slider GO](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/toggle_demo/img/uGuiToggleSliderGo.png)

Toggle Slider对象包含的Toggle Slider组件是唯一一个直接和控件有关的脚本。代码可在[github](https://github.com/jaredoc/unity-ugui/blob/master/toggle_demo/ugui_project/Assets/ToggleSlider/ToggleSlider.cs)查阅，编写起来很简单。

### Animation ###

![uGui Animation](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/toggle_demo/img/uGuiAnimation.png)

所有效果都使用Animation组件实现，全部用动画是为了偷懒，毕竟效果怎么实现都可以，这里仅作演示。动画包含四条曲线，分别用于控制两只twitter小鸟、蓝色背景透明度和滑块左右移动。这里简单提几个要点。

- 动画的**反向播放**只需要将AnimationState.speed设为-1。
- 拖拽滑块时，动画暂停，根据鼠标位移**逐帧**设置动画时间，然后Sample动画。拖拽停止时恢复动画。
- 在动画里改变透明度时，Image组件不会自动更新，需要添加一个[ColorWatcher](https://github.com/jaredoc/unity-ugui/blob/master/toggle_demo/ugui_project/Assets/Scripts/ColorWatcher.cs)组件，自己触发Image.color的setter。
- 动画设为ClampForever，因为Once无法在AnimationState中保留最后一帧的状态。

### Event ###

![uGui Handle Go](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/toggle_demo/img/uGuiHandleGo.png)

事件使用两个**Event Trigger**组件进行响应。一个在Toggle Slider对象里，负责响应OnPointerUp，实现当点击控件时，调用ToggleSlider.Toggle()。另一个在Handle对象里(如图)，负责响应Drag事件，实现当拖动时调用ToggleSlider.OnDrag()。

在此遇到了一个蛋疼的问题，Event Trigger的**Drop**事件在这里无效，又没有单独的**DragEnd**事件，因此只好在Handle上增加OnPointerUp事件来监听拖动是否结束。如此一来，Handle的OnPointerUp就会把上层控件的OnPointerUp事件拦截掉……我希望Unity能提供类似**冒泡**的机制，这样一来我就能在Handle上添加一个脚本，只对拖拽结束进行响应，如果是单击事件就冒泡到上层控件进行处理。

最终我的做法是，Handle的OnPointerUp事件也由ToggleSlider.OnPointerUp()响应，OnPointerUp内部通过dragging标记来判断是拖动结束还是单击。

不足
----------
- Event Trigger没有冒泡机制，子控件如果不处理事件，没办法抛给父控件处理。
- ImageMask没能选择alpha threshold。

存在的Bug
----------

这段时间的测试遇到过几个问题：

- 经常警告"Material uGUI/Stencil Mask doesn't have stencil ... SendWillRenderCanvases()"。有时会导致Image无法显示，要换过一次Sprite之后才正常。
- 两个Hierarchy内平级并且相邻的ImageMask，都选中DrawImage，结果上面一个会挡住下面一个。需要在两个中间插入一包含CanvasRenderer的GO才行，GO可以deactivated。
- 当我制作NGUI版本时，从uGui复制了一份出来再做修改。做到一半时我发现Hierarchy多了一个不含子物体的副本，当我选中控件时副本会同时被选中。于是我重启Unity，发现Unity已经死锁无法关闭，强制结束后项目损坏，只要一打开就crash，手动删除scene后才恢复正常。估计是我在继承树上混用NGUI/uGui，或者uGui未剔除干净引起的，已向官方反馈。

<a name="compare_ngui"></a>和NGUI对比
----------

作为对比，我也用NGUI的测试版(3.6.4b)做了一样的demo，花了不少时间。uGui的事件问题也在NGUI里遇到了，甚至更严重，此外还有其它问题。

![NGUI Toggle Slider](https://raw.githubusercontent.com/jaredoc/unity-ugui/master/toggle_demo/img/NGuiToggleSlider.gif)

- **NGUI的padding设置挺繁琐的**，uGui只要Rect Transform点下stretch，Left/Top/Right/Bottom全写20就行。

  添加padding时，我试着创建一个UIWidget，然后设置Anchors为Unified，然后依次设置Left/Top/Right/Bottom为Target's Left/Top/Right/Bottom，然后数值填入20/-20/20/-20才行。
- **NGUI添加Toggle有点复杂**，uGui只要Hierarchy里Create一个就完事了。

  创建调试用的Current Value时，找不到NGUI的Toggle组件，后来输入名称才找到，但还是不太会用。后来想到Examples里有toggle的prefab可以用，拖进Scene后对比了下发现NGUI的实现方式比uGui复杂了些，难以手工创建出来。看来Project里要把NGUI这些常见库都备好才行。

- **NGUI设置Anchor有点失败**

  将Toggle的prefab实例化到scene里后，设置了很久都没能让Toggle自动居中。**难道这个Toggle的尺寸如果是动态的，NGUI就没办法自动居中？**或许是我对NGUI还不是很了解，最终我只能根据Toggle宽度算出坐标偏移。
  
- **NGUI没有Image Mask**
  
  所以这个版本没能加入那两个twitter的logo。这个怪不了NGUI，因为Unity的free版不提供访问stencil buffer的功能，因此第三方UI插件没办法实现比较好的clipping机制。

- **NGUI的UIEventTrigger无法获得事件参数**
  
  UIEventTrigger和uGui的EventTrigger类似，能够触发远程方法。但是NGUI不能传入动态事件参数，虽然能用UIEventTrigger.current获得当前事件，可UIEventTrigger对象其实没定义任何参数。要获得参数，只能自己写一个带有OnDrag的组件，附加到GameObject上，或者使用UIEventListener，总之就不支持可视化编辑，只能用代码动态绑定事件。

- **NGUI的UIEventListener无法响应停止拖动事件**
  
  为了解决前一个问题，我使用了UIEventListener来获得拖动参数。然而当我想响应停止拖动事件时，我发现还是要用回UIEventTrigger才行。如果用户不希望混用这两个脚本，那么只能自己写一个脚本。

小结
----------

uGui功能和用户体验方面都做的不错，是我看到过最贴近Unity风格的UI系统。稳定性方面有小问题，不过作为测试版可以理解，已经超过了我的预期(之前以为会和4.0的刚推出Mecanim一样bug一堆)。性能方面暂时不做测试，因为这个demo里的Sprite用的都是独立贴图，只有pro版Unity才支持自动packing。之后我会更换素材再进行更准确测试。


> [Jare](http://weibo.com/u/1751917933) @ [梦加网络](http://www.mechanist.co/cn/)

> 本文托管于 https://github.com/jaredoc/unity-ugui/tree/master/toggle_demo 欢迎Fork、提Issue或转载
