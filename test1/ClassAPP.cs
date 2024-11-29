using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace test1
{
    class ClassAPP : IExternalApplication
    {
        //先获取到最后编译成test2.dll这个文件时的实时路径，全局变量，供下方使用
        string path = typeof(ClassAPP).Assembly.Location;

        //关闭事件，revit关闭时，执行下面事件中代码
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        //开启事件，revit打开时，执行下面事件中代码，我们利用这里，载入我们的插件（以下代码为固定，只要改变参数就可以创建别按钮）
        public Result OnStartup(UIControlledApplication application)
        {
            //创建插件在revit界面中的菜单栏
            application.CreateRibbonTab("工具箱");

            //在菜单栏创建一个分栏
            var SJ = application.CreateRibbonPanel("工具箱", "工具栏1");

            var pushButton1 = new PushButtonData("工具1", "工具1", path, "test1.Class1");
            pushButton1.LargeImage = new BitmapImage(new Uri(path.Replace("test1.dll", "1.ico")));
            SJ.AddItem(pushButton1);


            return Result.Succeeded;
        }
    }
}
