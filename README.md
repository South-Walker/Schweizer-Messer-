UnityNote
===

unity学习笔记
---

> 彼时我见如来，此时如来见我

#### 2017-11-13

>```c#
>this.transform.position = Vector3.Lerp(aim, now, Time.deltaTime * Smooth);
>```
>* 线性插值方法，实际上是从now点出发在获得一个接近aim的比，now与aim距离之值 比上 返回值与aim距离之值等于第三个参数，用这个方法可以实现摄像机弹性的跟踪一个移动目标
>* Instantiate(Prefab);//生成预制体，返回值是生成的预制体的Transform属性（引用类型），可以使用其SetPrent方法设定父
>```c#
>this.transform.rotation = Quaternion.Euler(5, 0, 0);
>```
>* 要注意旋转这个属性不简单是一个三元数（可以用三元数实例化Quaternion）用的是欧拉旋转法，每个参数分别表示绕x，y，z轴旋转度数。<a href="http://blog.csdn.net/candycat1992/article/details/41254799" target="_blank">关于旋转的具体说明，有时间再看</a>

#### 2017-11-14

>```c#
>IAnimatorable.SetFloat(string name, float value, float dampTime, float deltaTime);
>```
>* 实现了IAnimatorable接口的类都有SetFloat这个实例方法，表示对该类中名字为name的属性赋值为value，后面两个参数可选，其中dampTime为到达value允许的最低时间（帧刷新需要时间），deltatime输入帧刷新时间，换句话说这一帧结束时的速度value满足
>```c#
>value = current + (target - current) * deltaTime / (dampTime + deltaTime); 
>```

>```c#
>Vector3 front = this.transform.forward;
>```
>* transform的forward这个属性返回的是当前对象z轴在大坐标上对应向量的单位向量，注意在Unity的注释里，是用颜色表示笛卡尔坐标轴的，blue axis指的就是z轴

#### 2017-11-15

>* 初步解决了基本的运动（前、左、右），但是当物体朝向（this.transform.forward）改变时原有算法失效。解决思路如下：
>* 以物体本身建系，表示物体朝向的矢量在方向上等于，过起点和终点（该帧起始和结束），圆心角为锐角的一段弧对应的圆在终点处的切线。
>* 同时，当帧间隔很短时，弧长也很短，可以近视做一条线段，对应的切线与线段重合，
>* 举个例子，当帧间隔很短时，物体朝向对应的矢量在方向上等于物体在这一帧内位移对应的矢量
>```c#
>this.transform.forward = nextposition - this.transform.position;
>```
>* 最后，要修改位移算法，因为此时物体朝向会改变，所以计算物体位移时以物体朝向为z轴建系后得到的位移矢量要换算成主坐标系的位移矢量，具体计算方法如下：
>* 首先声明的一点this.transform.forward得到的是单位矢量，长度恒为1，另其为α
>* 令以物体朝向为z轴得到的坐标系为小坐标系，先以小坐标系为准，用角度和速度得到参考位移矢量β，β在小坐标系z轴上投影为b，在数值上等于|α·β|，所以物体在z轴方向位移矢量等于|α·β|α
>* 假设矢量γ⊥α，所以有|γ·α|=0，γ可解，物体在伽马方向位移矢量等于|β·γ|γ
>* 故矢量β在大坐标系上等于|α·β|α+|β·γ|γ
>* (其实上面是算错了，多算了一个叉积导致只有当大小坐标系z轴方向相同时结果才正确，正确的算法见11-18笔记)
>* 但是，以上算法有个缺点，由于朝向已经和Direction属性绑定，而Direction是基于输入的，所以在无输入时，朝向恢复默认值。
>* 解决方法:
>```c#
>Rotation += newdirection;//之前相当于是用向量的方向来表示，现在是与direction值本身持续的时间和大小挂钩
>```

#### 2017-11-17

>```c#
>Vector3.Cross(nowforward, blueaxis) == Vector3.Cross(beforeforward, blueaxis);
>Vector3.Dot(nowforward, blueaxis) <= Vector3.Dot(beforeforward, blueaxis);
>```
>* 前者利用外积判断两单位向量是否在z轴同侧，后者利用内积判断nowforward与z轴成的角是否大于beforeforward与z轴成的角

#### 2017-11-18

>* 使前进方向与朝向挂钩的算法：
>```c#
>float nextx = _speed * Mathf.Cos(rad);
>float nextz = _speed * Mathf.Sin(rad);
>Vector3 front = this.transform.forward;
>Vector3 right = Vector3.Cross(front, new Vector3(0, 1, 0));
>Vector3 nextstep = nextz * front + nextx * right;
>```

#### 2017-11-19

>```c#
>public GameObject GameObject;
>GameObject.GetComponent<ScriptName>().AttributionOrMethor
>```
>* 用这种泛型形式在别的脚本中调用ScriptName脚本中的属性或方法

#### 2017-11-20

>* 生成exe的方法File=>Build Settings

>* 判断当前动画播放状态（AnimatorStateInfo类）
>```c#
>private bool haveJumped()
>{
>   var animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
>   if (animatorStateInfo.IsName("Jump") && animatorStateInfo.normalizedTime > 0.8)
>   {
>       willJump = false;
>       return true;
>   }
>   return false;
>}

#### 2017-11-22

>* 当需要做操控角色的碰撞检测时，用Character Controller比添加为刚体更为方便，此时，使this运动不再直接对this.transform.position赋值，而使用如下方法：
>```c#
>public CharacterController CharacterController;
>CharacterController = this.GetComponent<CharacterController>();
>CharacterController.move(GetNextStep());
>```
>* Character Controller与刚体重力不兼容，同时使用会造成物体震颤，解决方法是使用isGrounded判断是否在地面，如果不是，维护一个表示向下速度的属性，在帧末尾执行下落操作
>* Character Controller里Skin Width是个比较重要的属性，表示允许别的碰撞器进入多少距离，有待研究

#### 2017-12-8

>* <a href="http://blog.csdn.net/lishuzhai/article/details/48501171" target="_blank" title="dachuang">关于粒子效果相关参数说明</a>
>* Duration：动画持续时间（loop关闭）
>* Start Lifetime:粒子存活时间
>* Start Speed:粒子初速度
>* Start Size:初状态粒子大小
>* Emission（（气体）排放，散发）
    >* Rate:每秒发射粒子数量
>* Shape（发射器形状）
>* Color over Lifetime:随粒子存活时间改变的粒子颜色
>* Size over Lifetime:随粒子存活时间改变的粒子大小
>* Renderer（渲染器）

#### 2018-1-27

>* 用旋转矩阵解决应用在不同移动平台下的适应问题
    >* 首先明确一点，对任意向量（x;y;z）做出的有限次平移，缩放，旋转操作后得到的新向量（x';y';z'），可以用旋转矩阵（1）与（x;y;z;1）相乘得到（x';y';z';1），而旋转矩阵（1）可由4*4单位矩阵进行有限次初等行变化得到，只与变化有关，与向量本身无关
    >* 显然，由矩阵乘法的定义，（x;y;z;1）乘以4*4单位矩阵得到（x;y;z;1），即对不变成立
    >* 假设位移后向量为（x+a;y+b;z+c;1），显然，由初等变化与矩阵乘法，只需要对4*4单位矩阵进行3次第三类初等变换即R30(a),R31(b),R32(c)
    >* 假设释放后向量为（ax;by;cz;1），显然，由初等变化与矩阵乘法，只需要对4*4单位矩阵进行3次第2类初等变换即R0(a),R1(b),R2(c)
    >* 旋转的证明较为麻烦，首先应该说明的是，在三维空间中表示刚体的旋转有两种方法：欧拉角与四元数，因为现在用的是旋转矩阵，所以主要用到的是欧拉角
        >* 欧拉角：将物体的旋转拆分成分别以三个轴为转动轴的旋转
            >* 万向节死锁：gimbal万向节在一个平面恰好旋转90度后，存在两平面重合，即两转动轴重合，此刻万向节本身相当于只存在2个转动轴，只有2种转动方式，丧失了1个转动自由度。<a href="http://v.youku.com/v_show/id_XNzkyOTIyMTI=.html" target="_blank" title="dachuang">关万向节锁视频</a>
            >* 在unity里，特别的，定义了旋转父子关系是y-x-z，即先改变y的值，再改变x，最后改变z。当改变到x的值时使得z,y重合，即成为万向节锁，此时，对y与对z的旋转等效。
        >* 旋转矩阵：
            >* Rx(a):[1,0,0,0;0,cos(x),-sin(x),0;0,sin(x),cos(x),0;0,0,0,1]
                >*       1       0       0 0
                >*       0  cos(a) -sin(a) 0
                >*       0  sin(a)  cos(a) 0
                >*       0       0       0 1
            >* Ry(a):[cos(y),0,sin(y),0;0,1,0,0;-sin(y),0,cos(y),0;0,0,0,1]
                >*  cos(a)       0  sin(a) 0
                >*       0       1       0 0
                >* -sin(a)       0  cos(a) 0
                >*       0       0       0 1
            >* Rz(a):[cos(z),-sin(z),0,0;sin(z),cos(z),0,0;0,0,1,0;0,0,0,1]
                >*  cos(a) -sin(a)       0 0
                >*  sin(a)  cos(a)       0 0
                >*       0       0       1 0
                >*       0       0       0 1
            >* 按YXZ的顺序相乘:[cos(y)*cos(z)+sin(x)*sin(y)*sin(z),cos(z)*sin(x)*sin(y)-cos(y)*sin(z), cos(x)*sin(y),0;cos(x)*sin(z),cos(x)*cos(z),-sin(x), 0; cos(y)*sin(x)*sin(z) - cos(z)*sin(y), sin(y)*sin(z) + cos(y)*cos(z)*sin(x), cos(x)*cos(y),0;0,0,0,1]

#### 2018-1-30

>* OnGUI方法在渲染GUI的时候被调用（频繁的），在OnGUI内部使用的方法应该为GUI类的静态方法(也可以设置GUI类的静态属性)例如：
    >* GUI.matrix：设置自适应矩阵
    >* GUI.Button：设置按钮，这个方法有一个bool返回值，在被点击后返回True，捕捉后可以进行处理
    >* GUI.DrawTexture：设置材质（背景）
>* 由于OnGUI方法是频繁调用的，所以切换页面的逻辑就很显然了，即令旧脚本（实际上是MonoBehaviour类的一个子类的实例，可以通过GetComponents<MonoBehaviour>()得到所有脚本的数组）的enabled属性为false，新脚本的enabled属性为true，此时渲染新页面的GUI而非旧页面的，即实现换页（不更换场景）
>* PlayerPrefs这个类下有静态方法可以支持本地持久化保存与读取（SetInt,GetInt,SetFloat,GetFloat,SetString,GetString）
    >* 在不同平台下保存的方法不同，在windows下是以注册表的形式写入的，查看方法是win+r输入regedit，数据存储在HKEY_CURRENT_USER->Software->[CompanyName]->[ProjectName]
    >* CompanyName与ProjectName的查看方法：File=>Build Setting=>Player Settings，在最上方

#### 2018-1-31

>* 切换场景方法：
    >* Application.LoadLevel("SceneName");//5.0版本以前
    >* SceneManager.LoadScene("SceneName");//5.0版本以后，需要引用UnityEngine.SceneManagement命名空间

#### 2018-2-1

>* 移动端拖动页面不换页，松开手指换页的方法
>```c#
>        //判断是否触控
>        if (Input.touchCount > 0)
>        {
>            //获取一个触控点    
>            Touch touch = Input.GetTouch(0);
>            //按钮按下时的回调方法
>            if (touch.phase == TouchPhase.Began)
>            {
>                touchPoint = touch.position;//记录down点
>                prePositon = touch.position;//记录down点                
>            }
>            else if (touch.phase == TouchPhase.Moved)
>            {
>                //touch.position.y - preP          
>                float newPositonY = positionY - touch.position.y + prePositon.y;
>                //positionY的范围-480*7-0
>                positionY = (newPositonY > 0) ? 0 : (newPositonY > (-480 * 7) ? newPositonY : (-480 * 7));
>                //记录上一次的触控点
>                prePositon = touch.position;
>            }
>            else if (touch.phase == TouchPhase.Ended)
>            {
>                //手指抬起来时，图片开始自动移动
>                isMoving = true;
>                //计算从触控开始到抬起的距离
>                currentDistance = (touch.position.y - touchPoint.y) / scale;
>                //执行移动方法      
>                step = (Mathf.Abs(currentDistance) > 150.0f) ? (currentDistance > 0 ? 1 : (-1)) : 0;
>                //计算当前移动步径
>                moveStep = (Mathf.Abs(currentDistance) > 150.0f) ? (currentDistance > 0 ? -Mathf.Abs(moveStep) : Mathf.Abs(moveStep)) : (currentDistance > 0 ? Mathf.Abs(moveStep) : -Mathf.Abs(moveStep));
>                //计算当前编号
>                int newIndex = currentIndex + step;
>                //如果编号超出边界,进行处理
>                //修改当前索引值currentIndex，在OnGUI里渲染
>                currentIndex = newIndex;
>            }
>```

#### 2018-2-1

>* 注意直接在界面上对实例变量赋值的优先度是高于在脚本赋值的

#### 2018-2-4

>* OnGUI最后只在一个挂载脚本中存在，不然可能重叠

#### 2018-2-5

>* 一般的，在unity的Inspector界面中输入的变量只有其特定的几种，为了能够输入自己声明的类型，需要在类型声明前将其对象序列化
>```c#
>	[System.Serializable]
>	public class className
>	{
>        //your class
>   }
>```
>* 在unity中，如果一个物体本身是预置体（Prefab）的副本（名字是蓝色），其具有回复成原先状态的功能，当预置体被删除后，这个功能失效，物体处于Missing状态（名字是棕红色），解决方法是选中物体=>GameObject=>Break Prefab Instance，由于这个物体不再被当做一个Prefab的实例，故功能消失，从Missing状态恢复

#### 2018-3-6

>* 切换场景方法（异步版本）：
    >* AsyncOperation ChangeProg = SceneManager.LoadSceneAsync("FantasyPlanetScene");
    >* 通过修改ChangeProg的allowSceneActivation属性决定切换场景时机，同时也提供了查询加载进度的方法
>* 如需编码入一个文本文件，可以直接放在Assets目录下，在脚本中，以TextAsset形式获取
>* 在unity下，Visual Studio更像是作为编辑器在被使用，部分引用需要手动添加到unity文件中（区别于直接添加服务引用）使用部分引用时要留意跨平台时是否能用（如System.Drawing）
>* Image类的Save实例方法使用会导致unity崩溃，理由是使用了Visual Studio自带的System.Drawing.dll（将它复制到了unity的Asset里）而实际上能被unity接受的System.Drawing.dll存在于C:\Program Files\Unity\Editor\Data\Mono\lib\mono\2.0\System.Drawing.dll，明显大小不一样，应该是经过了重写以适应mono平台，将其复制到Asset里即可
>* 不过就算这样做了。。。实际上在移动端也没办法使用System.Drawing.dll

#### 2018-3-7

>* 今天开始研究unity的ui
>* 在不同环境下有时候会希望同一个方法有不同行为，可以以添加宏定义的方式实现
>```c#
>	public void Quit () 
>	{
>		#if UNITY_EDITOR
>       //停止调试
>		UnityEditor.EditorApplication.isPlaying = false;
>		#else
>       //退出应用程序
>		Application.Quit();
>		#endif
>	}
>```
>* 公共变量在Inspector中能被修改的值可以通过添加特性的方法被约束
>```c#
>    [Range(10,100)]
>    public int resolution = 10;//在代码中的修改不受此限制
>```
>* Time.time从游戏开始经过的时间，用秒表示

#### 2018-3-7

>* 关于协程
    >* 协程本质上是利用迭代器IEnumerator实现的跨帧保留数据，工作的方法
    >* StartCoroutine();是协程最重要的方法，其可以传入一个返回迭代器的方法（可以是函数本身，也可以是字符串形式的函数名，后者便于用StopCoroutine();终止），然后在每一帧会迭代一次迭代器
    >* 由于迭代器本身不是多线程的实现，所以协程也不是多线程的
>* 动态批处理Dynamic batching：当使用满足一定规则的相同的材质时，unity可以将其保存复用以节约cpu与gpu交互的次数，可以通过点击Game界面Stats查看影响（Save by batching与Batches）
>* 用一个Material实例来实例化Material的意思是创建其的一个深拷贝副本

#### 2018-3-9

>* ui界面的层次
    >* Canvas承载所有ui元素
    >* Panel，视ui复杂程度，下可能还有一个image
>* 部分动画文件可能提前写死了执行动画的部件的名字（path），可以用文本形式打开进行修改

#### 2018-3-11

>* 在元素内部对其子元素布局，可以通过添加Layout Group组件的形式（本次使用的是Grid Layout Group）本质上是基于网格的布局
>* 修改UGUI类的Text类UI内的Text组件值，需要引用using UnityEngine.UI命名空间
>* 在代码中查找元素最简单的方法是使用Find函数，它是位于Transform类的实例方法，如果直接输入名字代表检索子元素内名字为输入字符的元素，也可以用路径的形式检索孙及以下节点，注意它本身对性能开销极大，不应该在update及类似函数中使用（作为替代，可以在类中声明变量，将其保存在内存中）

#### 2018-3-15

>* Auto Layout System 自动布局系统
    >* 自动布局系统可分为Layout Controllers（父物件，控制器）与Layout Elements（子物件，元素）
    >* 以垂直布局工具为例，其控制器组件名为Vertical Layout Group，对应的元素组件名为Layout Element
    >* unity2017更改了一些设定，似乎如果不在控制器中勾选Child Controls Size的对应选项，子元素中就无法使用Min => Preferred => Flexible的三级调节
    >* 对Min，Preferred，Flexible的理解
        >* Min：元素所能接受的最小大小，如果子元素Min之和超过控制器的对应大小，元素会超出控制器
        >* Prefferred：在满足Min的条件下如果控制器还有空间，会把空间用来满足Prefferred，值得注意的是如果空间不足以完全分配，会按一定算法分配，此时，请求的Prefferred越多得到的空间越多
        >* Flexible：在满足前两者的情况下，继续进行分配，值得注意的是前两者都是基于像素，而Flexible是基于比例
    >* 在控制器的Inspector最下方中，可以切换为Layout Properties查看该控制器内元素Min，Preferred，Flexible之和，以用来确定控制器对象的宽高
    >* 在控制器中修改Child Alignment属性可以修改布局，Upper、Middle、Lower决定元素在Y轴上的对齐方向，Left、Center、Right决定元素在X轴上的对齐方向，两者共同形成一个九宫格对齐方案
>* 关于Resources.Load()方法，一定要注意，其所加载的对象路径是以Assets下名为Resources的目录为根目录的相对路径

#### 2018-3-16

>* 关于Awake(),Start(),OnEnable()
    >* 在官网文档上，Awake(),Start()的执行时机是First Scene Load，即场景初次加载时，而Start()的执行时机是Befor the first frame update,即第一次Update()函数执行前，显然前两者优先级较Start()大，而Awake在场景加载完后就发生，优先级大于Enable
    
#### 2018-3-18

>* 如果在动画中只是想实现简单的位移，可以用animtor的过渡自动生成，只需要在终末animtion中写好状态就好

#### 2018-3-19

>* Light Probes：灯管探测器，是一个网格渲染器（Mesh Renderer）中的属性，与光反射有关，在烘培结束后对场景内非静态物体继续渲染
>* DontDestroyOnLoad(Object target)：在unity中，要加载新scene时，首先会先Destroy旧场景的所有对象，但是如果旧场景中有对象被传入了DontDestroyOnLoad方法，那么其将被保留到新场景中，注意，该方法需要保证在回复旧场景时不在生成新的target不然target对象会越来越多

#### 2018-3-20

>* Input.acceleration：acceleration这个词在物理里是加速度的意思，在unity里用来获取手机感受到的重力方向

#### 2018-3-21

>* Audio Listener与Audio Source：Audio Source是音源，Audio Listener是音频侦听器，场景内可以有多个音源，但是原则上只能有一个音频侦听器

#### 2018-3-22

>* OnTriggerEnter(Collider other):碰撞两方存在刚体和isTigger属性时，可以捕捉碰撞other值是挂载脚本者外的一方
>* OnMouseDown():挂载在物体上，在PC端，这个事件会在鼠标点击挂载物体后触发，在移动端则是手指按上挂载物体后。
>* Camera.main返回第一个标签为MainCamera的摄像机，也就是正起作用的摄像机

#### 2018-3-24

>* .physicmaterial:物理材质文件，本身是个文本文件，可以打开手动修改
    >* dynamicFriction:动摩擦系数
    >* staticFriction:静摩擦系数
    >* bounciness:弹力系数

#### 2018-3-25

>```c#
>//这个属性是指是否冻结物体，值是一个枚举类型
> Rigidbody rigidbody.constrains=RigidbodyConstrains.FreezePosition;
>```

#### 2018-4-17

>* shader：着色器，操作GPU绘制图形的接口
>* unity中的shader是对shader的再封装
>* shader language:与编程语言操作cpu对应，着色器语言就是操作gpu的语言，但是封装层级较编程语言低
    >* 常见的着色器语言有：HLSL(DirectX)、GLSL（OpenGL Shading Language）、Cg(NVIDIA)
    >* 在unity shaer中,使用的着色器语言是"Cg/HLSL"或者"GLSL"，因为多封装了一层，所以实际上并不是真正对应着实际的着色器语言
>* unity 4.x中默认是没有天空盒的，而unity 5.x默认自带一个天空盒，可以在Window=>Lighting=>Settings设置
>* 创建自己的着色器：Project（右击）=>Create=>Shader，其中一共有四种模板：
    >* Standard Surface Shader:包含标准光照的模板
    >* Unlit Shader:不包含光照但包含雾效的基本的顶点/片元着色器
    >* Image Effect Shader:实现屏幕后处理效果
    >* Compute Shader:利用GPU的并行性来进行与常规渲染流水线无关的计算
>* shader本身是个文本文件，一般使用的是Unlit Shader

#### 2018-4-30

>* Dropdown控件，如果clear后手动添加数据，设置初始值为0时会因为其默认的初始值已经是0，所以不会发生ValueChange事件，解决方法是设置value值为-1
>* 滚动条的制作
    >* 放入Scrollbar控件
    >* 给想要滚动的的控件设置ScrollRect与Mask
        >* ScrollRect绑定Scrollbar表示其带领控件的子元素运动
        >* Mask防止不应出现的内容出现在控件外
    >* 如果目标内容是不定长的text，那么需要在代码端控制内容的Height防止显示不全
    >```c#
    >t_TextEvent.text = text;
    >float f = t_TextEvent.preferredHeight;
    >t_TextEvent.rectTransform.sizeDelta = new Vector2(t_SequenceOrTrackName.rectTransform.rect.width, f);
    >```

#### 2018-5-1

>* OnAudioFilterRead(float[] data, int channels);Unity的事件接口，当前挂载控件上有Audio Listener时会生效，每一帧的数据存放在data中，可以修改

#### 2018-7-31

>* 考研空闲刷刷OpenGL
>* freeglut、glew、GLTools、glut的环境已经丢百度网盘了，需要的时候去下，把lib与h放到VS对应位置就好了，gltools记得要自己编译一份lib。
>* 在win32环境下使用glut库需要在初始声明
>'''c
>#define GLUT_DISABLE_ATEXIT_HACK
>'''
>* 在创建OpenGL时使用空的c++控制台应用程序模板，预编译头可加可不加，记得右击解决方案，点击属性，链接器，输入，向附加依赖项添加opengl32.lib，glut32.lib和glu32.lib