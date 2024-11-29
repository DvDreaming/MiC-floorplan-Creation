using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Document = Autodesk.Revit.DB.Document;
using MessageBox = System.Windows.MessageBox;

namespace test1
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class1");
            tr_g.Start();


            //// 原来存在问题的代码-----------------------------------------
            //// example_1
            //// 2-dimensional boundary points of module_1
            //UV point_1 = new UV(0, 0);
            //UV point_2 = new UV(10, 0);
            //UV point_3 = new UV(10, 20);
            //UV point_4 = new UV(0, 20);
            //// 2-dimensional boundary points of module_2
            //UV point_5 = new UV(10, 0);
            //UV point_6 = new UV(20, 0);
            //UV point_7 = new UV(20, 20);
            //UV point_8 = new UV(10, 20);

            //修改后的代码----------------------------------------------------
            //主要是对于二维点做加减乘除，也就是二维点的点移动
            //module_1对应的点
            UV point_1 = new UV(0, 0);
            point_1 = point_1 - new UV(0.492, 0.492);
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_3 = point_2 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);
            //module_2对应的点
            UV point_5 = point_2;
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);


            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");
            // create rooms in the modules
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");
            // create a door between the bedroom and the living room
            FamilyInstance Door_1 = Utils.CreateDoorBetweenRooms(doc, bedroom, living_room, level);
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");

            tr_g.Assimilate();
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class Class2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class2");
            tr_g.Start();

            ////原代码-------------------------------------------------------------
            //UV point_1 = new UV(0, 0);
            //point_1 = point_1 - new UV(0.492, 0.492);
            //UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            //UV point_3 = point_2 + new UV(0, 20) + new UV(0, 0.492 * 2);
            //UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            //UV point_5 = point_2;
            //UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            //UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            //UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            //修改后的代码-------------------------------------------------------------
            //主要是对于二维点做加减乘除，也就是二维点的点移动
            // Define points for module_1 with doubled north-south length
            UV point_1 = new UV(0, 0);
            point_1 = point_1 - new UV(0.492, 0.492);
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);

            // Double the north-south length by extending the V coordinate of points 3 and 4
            UV point_3 = point_2 + new UV(0, 20 * 2) + new UV(0, 0.492 * 2); // doubled north-south length
            UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // module_2 points remain the same
            UV point_5 = point_2;
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);



            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");

            // create rooms in the modules
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");
            // create a door between the bedroom and the living room
            FamilyInstance Door_1 = Utils.CreateDoorBetweenRooms(doc, bedroom, living_room, level);
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");

            tr_g.Assimilate();
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    public class Class3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class3");
            tr_g.Start();

            ////原代码-------------------------------------------------------------
            //UV point_1 = new UV(0, 0);
            //point_1 = point_1 - new UV(0.492, 0.492);
            //UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);

            //// 将北南方向的长度增加两倍，即由 20 英尺改为 40 英尺
            //UV point_3 = point_2 + new UV(0, 40) + new UV(0, 0.492 * 2);
            //UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            //UV point_5 = point_2;
            //UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            //UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            //UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);



            ////修改后的代码1-------------------------------------------------------------
            //// Move module_1 60 feet to the west (subtract 60 from X coordinates of all points of module_1)
            //UV move_vector = new UV(-60, 0);

            //UV point_1 = new UV(0, 0);
            //point_1 = point_1 - new UV(0.492, 0.492) + move_vector;
            //UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);

            //UV point_3 = point_2 + new UV(0, 40) + new UV(0, 0.492 * 2);  // doubled length in north-south direction
            //UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            //// No movement for module_2
            //UV point_5 = point_2;
            //UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            //UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            //UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);




            //修改后的代码2-------------------------------------------------------------
            // 能否LLMs 处理模块位置的变化（例如，module_1向西移动 60 英尺）
            UV move_vector = new UV(-60, 0);

            UV point_1 = new UV(0, 0);
            // module_1向西移动 60 英尺即，X轴-60英尺
            point_1 = point_1 - new UV(0.492, 0.492) + move_vector;  
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            //南北方向长度加倍
            UV point_3 = point_2 + new UV(0, 40) + new UV(0, 0.492 * 2); 
            UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // module_2不动
            UV point_5 = new UV(10.76, 0); 
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);




            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");

            // create rooms in the modules
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");
            // create a door between the bedroom and the living room
            FamilyInstance Door_1 = Utils.CreateDoorBetweenRooms(doc, bedroom, living_room, level);
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");

            tr_g.Assimilate();
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    public class Class4 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class4");
            tr_g.Start();

            UV move_vector = new UV(-60, 0);
            UV point_1 = new UV(0, 0);
            point_1 = point_1 - new UV(0.492, 0.492) + move_vector;  // Apply movement to module_1
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_3 = point_2 + new UV(0, 40) + new UV(0, 0.492 * 2);  // doubled length in north-south direction
            UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);
            // No movement for module_2
            UV point_5 = new UV(10.76, 0);  // Starting position for module_2, no move_vector applied
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");


            // create rooms in the modules
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");
            // create a door between the bedroom and the living room
            FamilyInstance Door_1 = Utils.CreateDoorBetweenRooms(doc, bedroom, living_room, level);
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");


            //新增代码-------------------------------------------------------------
            FamilyInstance Window_3 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "south", "window");

            tr_g.Assimilate();
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    public class Class5 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class5");
            tr_g.Start();

            // Move module_1 60 feet to the west (subtract 60 from X coordinates of all points of module_1)
            UV move_vector = new UV(-60, 0);

            UV point_1 = new UV(0, 0);
            point_1 = point_1 - new UV(0.492, 0.492) + move_vector;  // Apply movement to module_1
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);

            UV point_3 = point_2 + new UV(0, 40) + new UV(0, 0.492 * 2);  // doubled length in north-south direction
            UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // No movement for module_2
            UV point_5 = new UV(10.76, 0);  // Starting position for module_2, no move_vector applied
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");


            // create rooms in the modules
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");
            // create a door between the bedroom and the living room
            FamilyInstance Door_1 = Utils.CreateDoorBetweenRooms(doc, bedroom, living_room, level);
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");

            FamilyInstance Window_3 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "south", "window");


            // 假设墙厚度为300mm(0.984英尺)（墙厚的一半为150mm(0.492英尺)）------------------------------------------------------------------------------
            double half_wall_thickness = 0.492;
            // 在 module_1 的东南角创建一个浴室（长4英尺，宽7英尺），南边和东边已有墙，不用扩展
            UV bathroom_southeast_point = module_1.GetSoutheastPoint() + new UV(0.492, 0.492);
            UV bathroom_southwest_point = new UV(bathroom_southeast_point.U - 7 - half_wall_thickness, bathroom_southeast_point.V);  // 仅向西扩展
            UV bathroom_northwest_point = new UV(bathroom_southwest_point.U, bathroom_southwest_point.V + 4 + half_wall_thickness);   // 仅向北扩展
            UV bathroom_northeast_point = new UV(bathroom_southeast_point.U, bathroom_southeast_point.V + 4 + half_wall_thickness);  // 不扩展

            // 创建西边和北边的墙
            Wall bathroom_north_wall = Utils.CreateWall(doc, bathroom_northeast_point, bathroom_northwest_point, level, "Bathroom North Wall");

            Wall bathroom_west_wall = Utils.CreateWall(doc, bathroom_northwest_point, bathroom_southwest_point, level, "Bathroom West Wall");

            // 创建浴室房间
            NowRoom bathroom = new NowRoom(doc, module_1, bathroom_southwest_point, bathroom_southeast_point, bathroom_northeast_point, bathroom_northwest_point, level, "Bathroom");

            // 在卧室和浴室之间创建一扇门
            FamilyInstance bathroom_door = Utils.CreateDoorBetweenRooms(doc, bedroom, bathroom, level);


            tr_g.Assimilate();
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    public class Class6 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var sel = uidoc.Selection;

            //禁止在非平面视图打开该功能
            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            //功能是创建DPF中的户型A
            XYZ orginPo = null;
            try
            {
                //让用户选择一个点
                orginPo = sel.PickPoint();
            }
            catch
            {
                return Result.Succeeded;
            }
            //基于这个起点，计算出墙生成需要的所有点
            var orginUV = new UV(orginPo.X, orginPo.Y);
            var Po1 = orginUV + new UV(0, 5290 / 304.8);
            var Po2 = Po1 + new UV(5690 / 304.8, 0);
            var Po3 = Po2 + new UV(0, -1708 / 304.8);
            var Po4 = Po3 + new UV(-2380 / 304.8, 0);
            var Po4_up = Po4 + new UV(0, 1708.8 / 304.8);
            var Po5 = Po4 + new UV(-980 / 304.8, 0);
            var Po5_R = Po5 + new UV(0, -1020 / 304.8);

            var orginPoAndPo2Cenren = orginUV + new UV(0, 2565 / 304.8);
            var orginPoAndPo2Cenren2 = orginPoAndPo2Cenren + new UV(3560 / 304.8, 0);
            var orginPoAndPo2Cenren2_down = orginPoAndPo2Cenren2 + new UV(0, -2565 / 304.8);
            var Po6 = orginPoAndPo2Cenren2 + new UV(2130 / 304.8, 0);
            var Po7 = Po6 + new UV(0, -2565 / 304.8);
            var Po8 = Po7 + new UV(-5690 / 304.8, 0);

            var Po9 = Po2 + new UV(280 / 304.8, 0);
            var Po10 = Po9 + new UV(0, -1738 / 304.8);
            var Po11 = Po9 + new UV(2640 / 304.8, 0);
            var Po12 = Po11 + new UV(0, -2050 / 304.8);

            var Po13 = Po12 + new UV(0, -1190 / 304.8);
            var Po14 = Po13 + new UV(0, -2044 / 304.8);
            var Po15 = Po14 + new UV(-2637 / 304.8, 0);
            var Po16 = Po15 + new UV(0, 2630 / 304.8);

            var tr_g = new TransactionGroup(doc, "Test-Class6");
            tr_g.Start();
            //创建墙
            var wall_Windows1 = Utils.CreateWall(doc, orginUV, Po1, level, "wall", 120);
            var wall_Windows2 = Utils.CreateWall(doc, Po1, Po2, level, "wall", 120);
            Utils.CreateWall(doc, Po2, Po3, level, "wall", 120);
            var wall_door2 = Utils.CreateWall(doc, Po4, Po3, level, "wall", 120);
            Utils.CreateWall(doc, Po4, Po4_up, level, "wall", 120);
            var wall_door1 = Utils.CreateWall(doc, Po5, Po4, level, "wall", 120);
            Utils.CreateWall(doc, Po5, Po5_R, level, "wall", 120);

            var wall_door3 = Utils.CreateWall(doc, Po6, orginPoAndPo2Cenren, level, "wall", 120);
            Utils.CreateWall(doc, orginPoAndPo2Cenren2, orginPoAndPo2Cenren2_down, level, "wall", 120);
            Utils.CreateWall(doc, Po6, Po7, level, "wall", 120);
            var wall_Windows4 = Utils.CreateWall(doc, Po7, Po8, level, "wall", 120);

            Utils.CreateWall(doc, Po9, Po10, level, "wall", 120);
            var wall_Windows3 = Utils.CreateWall(doc, Po9, Po11, level, "wall", 120);
            Utils.CreateWall(doc, Po11, Po12, level, "wall", 120);

            Utils.CreateWall(doc, Po13, Po14, level, "wall", 120);
            var wall_Windows5 = Utils.CreateWall(doc, Po14, Po15, level, "wall", 120);
            Utils.CreateWall(doc, Po15, Po16, level, "wall", 120);


            var tr = new Transaction(doc, "创建");
            tr.Start();

            //创建房间
            var array = new CurveArray();
            var line_1P0 = new XYZ(Po10.U, Po10.V, level.ProjectElevation);
            var line_2P0 = line_1P0 - XYZ.BasisY * 916 / 304.8;
            //因为有一个房间的墙是不闭合的，没办法创建房间，需要创建两条房间边界线和墙连接，让房间闭合
            var line1 = Line.CreateBound(line_1P0, line_2P0);
            var line2 = Line.CreateBound(new XYZ(Po12.U, Po12.V, level.ProjectElevation), new XYZ(Po13.U, Po13.V, level.ProjectElevation));
            array.Append(line1);
            array.Append(line2);
            //创建房间边界线
            doc.Create.NewRoomBoundaryLines(SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, line1.Origin)), array,uidoc.ActiveView);

            //基于用户选择的起点，计算出房间生成需要的所有点
            var BeaRoom1Po = orginUV + new UV(0.811339773203675, 0.584574864681856) * 2190 / 304.8;
            //创建房间
            var BeaRoom1 = doc.Create.NewRoom(level,BeaRoom1Po);
            //设置房间名称
            BeaRoom1.get_Parameter(BuiltInParameter.ROOM_NAME).Set("BEDROOM 1");
            //创建房间标记
            doc.Create.NewRoomTag(new LinkElementId(BeaRoom1.Id), BeaRoom1Po,uidoc.ActiveView.Id);

            var BeaRoom2Po = orginUV + new UV(0.970295726275996, 0.241921895599668) * 4800 / 304.8;
            var BeaRoom2 = doc.Create.NewRoom(level, BeaRoom2Po);
            BeaRoom2.get_Parameter(BuiltInParameter.ROOM_NAME).Set("BEDROOM 2");
            doc.Create.NewRoomTag(new LinkElementId(BeaRoom2.Id), BeaRoom2Po, uidoc.ActiveView.Id);

            var MasterRoomPo = orginUV + new UV(0.325568154457158, 0.945518575599317) * 4300 / 304.8;
            var MasterRoom = doc.Create.NewRoom(level, MasterRoomPo);
            MasterRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set("MASTER ROOM");
            doc.Create.NewRoomTag(new LinkElementId(MasterRoom.Id), MasterRoomPo, uidoc.ActiveView.Id);

            var BathRoomPo = orginUV + new UV(0.707106781186548, 0.707106781186547) * 6300 / 304.8;
            var BathRoom = doc.Create.NewRoom(level, BathRoomPo);
            BathRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set("BATHROOM");
            doc.Create.NewRoomTag(new LinkElementId(BathRoom.Id), BathRoomPo, uidoc.ActiveView.Id);

            var LivingRoomPo = orginUV + new UV(0.945518575599317, 0.325568154457156) * 7878.5 / 304.8;
            var LivingRoomP = doc.Create.NewRoom(level, LivingRoomPo);
            LivingRoomP.get_Parameter(BuiltInParameter.ROOM_NAME).Set("Living ROOM");
            doc.Create.NewRoomTag(new LinkElementId(LivingRoomP.Id), LivingRoomPo, uidoc.ActiveView.Id);

            //创建门
            //获得项目中默认的门类型
            var DefaultDoosType = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_Doors)
                            .WhereElementIsElementType()
                            .LastOrDefault();
            //激活门类型
            (DefaultDoosType as FamilySymbol).Activate();
            //基于用户选择的起点，计算出门生成需要的所有点
            var door1Po = orginPo + new XYZ(0.615537385136683, 0.788107687755357, 0) * 4550 / 304.8;
            //创建门
            var door1 = doc.Create.NewFamilyInstance(door1Po, DefaultDoosType as FamilySymbol, wall_door1, StructuralType.NonStructural);

            var door2Po = orginPo + new XYZ(0.777145961456971, 0.629320391049837, 0) * 5696 / 304.8;
            var door2 = doc.Create.NewFamilyInstance(door2Po, DefaultDoosType as FamilySymbol, wall_door2, StructuralType.NonStructural);

            var door3Po = orginPo + new XYZ(0.758157178415266, 0.652071846361582, 0) * 4035 / 304.8;
            var door3 = doc.Create.NewFamilyInstance(door3Po, DefaultDoosType as FamilySymbol, wall_door3, StructuralType.NonStructural);

            var door4Po = orginPo + new XYZ(0.848048096156426, 0.529919264233204, 0) * 4840 / 304.8;
            var door4 = doc.Create.NewFamilyInstance(door4Po, DefaultDoosType as FamilySymbol, wall_door3, StructuralType.NonStructural);

            //创建窗
            //获得项目中默认的窗类型
            var DefaultWindowsType = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_Windows)
                            .WhereElementIsElementType()
                            .FirstOrDefault();
            //激活窗类型
            (DefaultWindowsType as FamilySymbol).Activate();
            //基于用户选择的起点，计算出窗生成需要的所有点
            var Windows1Po = orginPo + new XYZ(1.8002682838121E-15, 1, 0) * 4820 / 304.8;
            //设置窗台高度1200mm
            Windows1Po = Windows1Po + XYZ.BasisZ * 1200 / 304.8;
            //创建窗
            var Windows1 = doc.Create.NewFamilyInstance(Windows1Po, DefaultWindowsType as FamilySymbol, wall_Windows1, StructuralType.NonStructural);

            var Windows2Po = orginPo + new XYZ(0.203444410389983, 0.97908649867163, 0) * 5400 / 304.8;
            Windows2Po = Windows2Po + XYZ.BasisZ * 1200 / 304.8;
            var Windows2 = doc.Create.NewFamilyInstance(Windows2Po, DefaultWindowsType as FamilySymbol, wall_Windows2, StructuralType.NonStructural);

            var Windows3Po = orginPo + new XYZ(0.640041200924222, 0.76834058926981, 0) * 6890 / 304.8;
            Windows3Po = Windows3Po + XYZ.BasisZ * 1200 / 304.8;
            var Windows3 = doc.Create.NewFamilyInstance(Windows3Po, DefaultWindowsType as FamilySymbol, wall_Windows2, StructuralType.NonStructural);

            var Windows4Po = orginPo + new XYZ(0.783583945903739, 0.621285924290842, 0) * 8520 / 304.8;
            Windows4Po = Windows4Po + XYZ.BasisZ * 1200 / 304.8;
            var Windows4 = doc.Create.NewFamilyInstance(Windows4Po, DefaultWindowsType as FamilySymbol, wall_Windows3, StructuralType.NonStructural);

            var Windows5Po = orginPo + new XYZ(1, 0, 0) * 1100 / 304.8;
            Windows5Po = Windows5Po + XYZ.BasisZ * 1200 / 304.8;
            var Windows5 = doc.Create.NewFamilyInstance(Windows5Po, DefaultWindowsType as FamilySymbol, wall_Windows4, StructuralType.NonStructural);

            var Windows6Po = orginPo + new XYZ(1, 0, 0) * 4640 / 304.8;
            Windows6Po = Windows6Po + XYZ.BasisZ * 1200 / 304.8;
            var Windows6 = doc.Create.NewFamilyInstance(Windows6Po, DefaultWindowsType as FamilySymbol, wall_Windows4, StructuralType.NonStructural);

            var Windows7Po = orginPo + new XYZ(1, 0, 0) * 7280 / 304.8;
            Windows7Po = Windows7Po + XYZ.BasisZ * 1200 / 304.8;
            var Windows7 = doc.Create.NewFamilyInstance(Windows7Po, DefaultWindowsType as FamilySymbol, wall_Windows5, StructuralType.NonStructural);


            //创建楼板
            //获得项目中默认的窗类型
            var DefaultfloorType = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_Floors)
                            .WhereElementIsElementType()
                            .FirstOrDefault();
            //基于用户选择的起点，计算出窗生成需要的所有点
            var floorPo1 = new XYZ(orginUV.U, orginUV.V, level.ProjectElevation);
            var floorPo2 = floorPo1 + XYZ.BasisY * 5290 / 304.8;
            var floorPo3 = floorPo2 + XYZ.BasisX * 8610 / 304.8;
            var floorPo4 = floorPo3 - XYZ.BasisY * 5290 / 304.8;
            //var loop = new CurveLoop();
            //loop.Append(Line.CreateBound(floorPo1, floorPo2));
            //loop.Append(Line.CreateBound(floorPo2, floorPo3));
            //loop.Append(Line.CreateBound(floorPo3, floorPo4));
            //loop.Append(Line.CreateBound(floorPo4, floorPo1));
            //Floor.Create(doc, new List<CurveLoop>() { loop }, DefaultfloorType.Id, level.Id);

            var loop = new CurveArray();
            loop.Append(Line.CreateBound(floorPo1, floorPo2));
            loop.Append(Line.CreateBound(floorPo2, floorPo3));
            loop.Append(Line.CreateBound(floorPo3, floorPo4));
            loop.Append(Line.CreateBound(floorPo4, floorPo1));
            //创建楼板
            doc.Create.NewFloor(loop,false);

            tr.Commit();

            tr_g.Assimilate();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Class7 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var sel = uidoc.Selection;

            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            Reference refe = null;
            try
            {
                //让用户选择一个房间
                refe = sel.PickObject( Autodesk.Revit.UI.Selection.ObjectType.Element);
            }
            catch
            {
                return Result.Succeeded;
            }
            //如果选择的不是房间，报错，提示
            var room = doc.GetElement(refe);
            if (!(room is Room) && !(room is RoomTag))
            {
                MessageBox.Show("Please select a room or roomtag for decoration.");
                return Result.Succeeded;
            }

            //如果是房间，获得房间的放置点
            Room newRoom = room as Room;
            if (room is RoomTag roomTag)
            {
                newRoom = roomTag.Room;
            }
            var RoomPoint = (newRoom.Location as LocationPoint).Point;


            var tr = new Transaction(doc, "Test-Class7");
            tr.Start();
            //获得房间的名称
            var RoomName = newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
            //如果是厕所（放置浴缸、马桶）
            if (RoomName.Contains("BATHR"))
            {
                //获得族名称为Closestool、Bathtub的族类型
                var closestool_Symbol  = new FilteredElementCollector(doc)
                                        .OfClass(typeof(FamilySymbol))
                                        .OfCategory(BuiltInCategory.OST_PlumbingFixtures)
                                        .WhereElementIsElementType()
                                        .Cast<FamilySymbol>()
                                        .FirstOrDefault(x => x.FamilyName == "Closestool"); 

                var bathtub_Symbol = new FilteredElementCollector(doc)
                                        .OfClass(typeof(FamilySymbol))
                                        .OfCategory(BuiltInCategory.OST_PlumbingFixtures)
                                        .WhereElementIsElementType()
                                        .Cast<FamilySymbol>()
                                        .FirstOrDefault(x => x.FamilyName == "Bathtub");

                //在房间的放置点创建浴缸、马桶
                var closestoolPo = RoomPoint + new XYZ(-0.910483370264832, -0.413545683656828, 0) * 940 / 304.8;
                var closestool = doc.Create.NewFamilyInstance(closestoolPo, closestool_Symbol, level, StructuralType.NonStructural);

                var bathtubPo = RoomPoint + new XYZ(-0.995037190209989, -0.099503719021003, 0) * 893 / 304.8;
                var bathtub = doc.Create.NewFamilyInstance(bathtubPo, bathtub_Symbol, level, StructuralType.NonStructural);
                MessageBox.Show("The bathtub and toilet are created in the room, please arrange them manually.");
            }
            //如果是卧室（放置床、床头柜）
            else if (RoomName.Contains("BED") || RoomName.Contains("MASTER"))
            {
                //Bed、Bedside Table
                var bed_Symbol = new FilteredElementCollector(doc)
                                        .OfClass(typeof(FamilySymbol))
                                        .OfCategory(BuiltInCategory.OST_Furniture)
                                        .WhereElementIsElementType()
                                        .Cast<FamilySymbol>()
                                        .FirstOrDefault(x => x.FamilyName == "Bed");

                var bedsidetable_Symbol = new FilteredElementCollector(doc)
                                        .OfClass(typeof(FamilySymbol))
                                        .OfCategory(BuiltInCategory.OST_Furniture)
                                        .WhereElementIsElementType()
                                        .Cast<FamilySymbol>()
                                        .FirstOrDefault(x => x.FamilyName == "Bedside Table");
                //在房间的放置点创建床、床头柜
                var bedPo = RoomPoint + new XYZ(-1, 0, 0) * 350 / 304.8;
                var bed = doc.Create.NewFamilyInstance(bedPo, bed_Symbol, level, StructuralType.NonStructural);

                var bedsidetablePo = RoomPoint + new XYZ(0.533031900011339, 0.84609514451408, 0) * 1125 / 304.8;
                var bedsidetable = doc.Create.NewFamilyInstance(bedsidetablePo, bedsidetable_Symbol, level, StructuralType.NonStructural);
                MessageBox.Show("The bed and bedside table are created in the room, please arrange them manually.");
            }
            else
            {
                MessageBox.Show("Currently only compatible with bedrooms and toilets.");
            }

            tr.Commit();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Class8 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var sel = uidoc.Selection;
            var view = uidoc.ActiveView;

            Reference refe = null;
            try
            {
                refe = sel.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            }
            catch
            {
                return Result.Succeeded;
            }

            var ele = doc.GetElement(refe);

            //以下着色和手动在Revit给元素着色操作一致
            using (Transaction trans = new Transaction(doc, "Test-Class8"))
            {
                trans.Start();

                // 创建一个新的覆盖设置对象
                OverrideGraphicSettings overrideSettings = new OverrideGraphicSettings();
                FillPatternElement solidFillPattern = new FilteredElementCollector(doc)
                                                    .OfClass(typeof(FillPatternElement))
                                                    .Cast<FillPatternElement>()
                                                    .FirstOrDefault(x => x.GetFillPattern().IsSolidFill);  // 获取实体填充图案
                // 设置表面、截面颜色，实体填充图案
                overrideSettings.SetSurfaceForegroundPatternColor(new Color(255, 0, 0));
                overrideSettings.SetSurfaceForegroundPatternId(solidFillPattern.Id);
                overrideSettings.SetSurfaceBackgroundPatternColor(new Color(255, 0, 0));
                overrideSettings.SetSurfaceBackgroundPatternId(solidFillPattern.Id);
                overrideSettings.SetCutForegroundPatternColor(new Color(255, 0, 0));
                overrideSettings.SetCutForegroundPatternId(solidFillPattern.Id);
                overrideSettings.SetCutBackgroundPatternColor(new Color(255, 0, 0));
                overrideSettings.SetCutBackgroundPatternId(solidFillPattern.Id);

                // 为指定的视图覆盖元素的图形设置
                view.SetElementOverrides(ele.Id, overrideSettings);

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class Class2_2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class2_2");
            tr_g.Start();

            ////原代码-------------------------------------------------------------
            //UV point_1 = new UV(0, 0);
            //point_1 = point_1 - new UV(0.492, 0.492);
            //UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            //UV point_3 = point_2 + new UV(0, 20) + new UV(0, 0.492 * 2);
            //UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            //UV point_5 = point_2;
            //UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            //UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            //UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            //修改后的代码-------------------------------------------------------------
            // Modified points for module_1 - doubled the length in north-south direction (V coordinate)
            UV point_1 = new UV(0, 0);
            point_1 = point_1 - new UV(0.492, 0.492);
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            // Modified point_3 and point_4 to extend northward (doubled the V value)
            UV point_3 = point_2 + new UV(0, 40) + new UV(0, 0.492 * 2); // Changed from 20 to 40
            UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // module_2 points remain unchanged
            UV point_5 = point_2;
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");

            // create rooms in the modules
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");
            // create a door between the bedroom and the living room
            FamilyInstance Door_1 = Utils.CreateDoorBetweenRooms(doc, bedroom, living_room, level);
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");

            tr_g.Assimilate();
            return Result.Succeeded;
        }
        
    }
    [Transaction(TransactionMode.Manual)]
    public class Class3_2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class1");
            tr_g.Start();

            // Convert 60 feet to meters (18.288 meters)
            double westwardShift = 60;

            // Modified points for module_1 - moved westward by 60 feet (18.288 meters)
            UV point_1 = new UV(-westwardShift, 0);  // Shifted westward
            point_1 = point_1 - new UV(0.492, 0.492);
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_3 = point_2 + new UV(0, 40) + new UV(0, 0.492 * 2);  // Height still doubled
            UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // module_2 points remain at original position
            UV point_5 = new UV(0, 0) + new UV(10.76, 0) + new UV(0.492 * 2, 0);  // Original position
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");

            // create rooms in the modules
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");

            // Since modules are no longer adjacent, we'll only create external doors and windows
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");
            // Add an east-facing door for the bedroom since it's now separate
            FamilyInstance Door_3 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "east", "door");

            tr_g.Assimilate();
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    public class Class4_2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class1");
            tr_g.Start();

            // Use 60 feet directly since Revit API uses feet as internal unit
            double westwardShift = 60;

            // Modified points for module_1 - moved westward by 60 feet
            UV point_1 = new UV(-westwardShift, 0);  // Shifted westward
            point_1 = point_1 - new UV(0.492, 0.492);
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_3 = point_2 + new UV(0, 40) + new UV(0, 0.492 * 2);  // Height still doubled
            UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // module_2 points remain at original position
            UV point_5 = new UV(0, 0) + new UV(10.76, 0) + new UV(0.492 * 2, 0);  // Original position
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");

            // create rooms in the modules
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");

            // Since modules are no longer adjacent, we'll only create external doors and windows
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");
            // Add an east-facing door for the bedroom since it's now separate
            FamilyInstance Door_3 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "east", "door");
            // Add a new south-facing window for the bedroom
            FamilyInstance Window_3 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "south", "window");

            tr_g.Assimilate();
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    public class Class5_2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class1");
            tr_g.Start();

            // Use 60 feet directly since Revit API uses feet as internal unit
            double westwardShift = 60;

            // Modified points for module_1 - moved westward by 60 feet
            UV point_1 = new UV(-westwardShift, 0);  // Shifted westward
            point_1 = point_1 - new UV(0.492, 0.492);
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_3 = point_2 + new UV(0, 40) + new UV(0, 0.492 * 2);  // Height still doubled
            UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // module_2 points remain at original position
            UV point_5 = new UV(0, 0) + new UV(10.76, 0) + new UV(0.492 * 2, 0);  // Original position
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");

            // Parameters for bathroom dimensions (in feet)
            double wallThickness = 0.984252;  // 300mm in feet
            // Add wall thickness only for new interior walls
            double bathroomNorthSouthLength = 4 + wallThickness;  // Interior 4 feet + one wall thickness (north wall)
            double bathroomEastWestLength = 7 + wallThickness;    // Interior 7 feet + one wall thickness (west wall)

            // Create bathroom in southeast corner of module_1
            NowRoom bathroom = new NowRoom(doc, module_1, "southeast", bathroomNorthSouthLength, bathroomEastWestLength, level, "Bathroom");
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");

            // Create doors and windows
            // Door between bedroom and bathroom
            FamilyInstance Door_1 = Utils.CreateDoorBetweenRooms(doc, bedroom, bathroom, level);

            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");
            // Add an east-facing door for the bedroom since it's now separate
            FamilyInstance Door_3 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "east", "door");
            // Add a south-facing window for the bedroom
            FamilyInstance Window_3 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "south", "window");

            tr_g.Assimilate();
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    public class Class8_2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the current document and UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Create red color
                Color redColor = new Color(255, 0, 0); // RGB values for red

                // Get all walls and floors
                FilteredElementCollector wallCollector = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Walls)
                    .WhereElementIsNotElementType();

                FilteredElementCollector floorCollector = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Floors)
                    .WhereElementIsNotElementType();

                // Get the solid fill pattern
                FillPatternElement solidFillPattern = new FilteredElementCollector(doc)
                    .OfClass(typeof(FillPatternElement))
                    .Where(x => (x as FillPatternElement).GetFillPattern().IsSolidFill)
                    .FirstOrDefault() as FillPatternElement;

                if (solidFillPattern == null)
                {
                    TaskDialog.Show("Error", "Could not find solid fill pattern");
                    return Result.Failed;
                }

                using (Transaction trans = new Transaction(doc, "Modify Element Graphics"))
                {
                    trans.Start();

                    // Create new override graphic settings
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    //ogs.SetProjectionFillColor(redColor);
                    //ogs.SetProjectionFillPatternId(solidFillPattern.Id);
                    ogs.SetSurfaceBackgroundPatternColor(redColor);
                    ogs.SetSurfaceBackgroundPatternId(solidFillPattern.Id);
                    ogs.SetSurfaceForegroundPatternId(solidFillPattern.Id);
                    ogs.SetSurfaceForegroundPatternColor(redColor);

                    // Apply to walls
                    foreach (Element wall in wallCollector)
                    {
                        doc.ActiveView.SetElementOverrides(wall.Id, ogs);
                    }

                    // Apply to floors
                    foreach (Element floor in floorCollector)
                    {
                        doc.ActiveView.SetElementOverrides(floor.Id, ogs);
                    }

                    trans.Commit();
                }

                TaskDialog.Show("Success", "Successfully modified element colors and fill patterns");
                return Result.Succeeded;
            }
            catch (System.Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Class2_3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class1");
            tr_g.Start();



            //module_1对应的点
            UV point_1 = new UV(0, 0);
            point_1 = point_1 - new UV(0.492, 0.492);
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_3 = point_2 + new UV(0, 40) + new UV(0, 0.492 * 2);
            UV point_4 = point_3 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);
            //module_2对应的点
            UV point_5 = point_2;
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);

            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");
            // create rooms in the modules
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");
            // create a door between the bedroom and the living room
            FamilyInstance Door_1 = Utils.CreateDoorBetweenRooms(doc, bedroom, living_room, level);
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");

            tr_g.Assimilate();
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    public class Class3_3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var view = uidoc.ActiveView;
            if (!(view is ViewPlan viewPlan))
            {
                MessageBox.Show("Please use in plan view.");
                return Result.Succeeded;
            }

            Level level = viewPlan.GenLevel;

            var tr_g = new TransactionGroup(doc, "Test-Class1");
            tr_g.Start();



            UV point_1 = new UV(-60, 0); // Subtract 60 feet from the X coordinate
            point_1 = point_1 - new UV(0.492, 0.492);
            UV point_2 = point_1 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_3 = point_1 + new UV(10.76, 20) + new UV(0.492 * 2, 0.492 * 2);
            UV point_4 = point_1 + new UV(0, 20) + new UV(0, 0.492 * 2);

            // Module 2's coordinates remain unchanged
            UV point_5 = new UV(10, 0);
            UV point_6 = point_5 + new UV(10.76, 0) + new UV(0.492 * 2, 0);
            UV point_7 = point_6 + new UV(0, 20) + new UV(0, 0.492 * 2);
            UV point_8 = point_7 + new UV(-10.76, 0) + new UV(-0.492 * 2, 0);


            // create modules
            Module module_1 = new Module(doc, point_1, point_2, point_3, point_4, level, "module_1");
            Module module_2 = new Module(doc, point_5, point_6, point_7, point_8, level, "module_2");
            // create rooms in the modules
            NowRoom bedroom = new NowRoom(doc, module_1, level, "Bedroom");
            NowRoom living_room = new NowRoom(doc, module_2, level, "Living room");
            // create a door between the bedroom and the living room
            FamilyInstance Door_1 = Utils.CreateDoorBetweenRooms(doc, bedroom, living_room, level);
            // create a door between the living room and the external space
            FamilyInstance Door_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "south", "door");
            // create a window between the living room and the external space
            FamilyInstance Window_1 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, living_room, level, "north", "window");
            // create a window between the bedroom and the external space
            FamilyInstance Window_2 = Utils.CreateDoorOrWindowBetweenRoomAndExternalSpace(doc, bedroom, level, "north", "window");

            tr_g.Assimilate();
            return Result.Succeeded;
        }

    }
    //[Transaction(TransactionMode.Manual)]
    //public class Class8_3 : IExternalCommand
    //{
    //    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    //    {
    //        UIDocument uidoc = uiapp.ActiveUIDocument;
    //        Document doc = uidoc.Document;

    //        // Get all Wall and Slab elements
    //        FilteredElementCollector collector = new FilteredElementCollector(doc);
    //        collector.OfClass(typeof(Wall))
    //                 .WhereElementIsNotElementType()
    //                 .OfCategory(BuiltInCategory.OST_Walls);
    //        collector.Intersect(new FilteredElementCollector(doc)
    //                                .OfClass(typeof(Floor))
    //                                .WhereElementIsNotElementType()
    //                                .OfCategory(BuiltInCategory.OST_Floors));

    //        // Iterate through elements and modify appearance
    //        foreach (Element element in collector)
    //        {
    //            // Get the material of the element
    //            Material material = element.LookupParameter("Material").AsElement() as Material;

    //            // Create a new material with red color and solid fill
    //            Material newMaterial = Material.Create(doc, "Red Solid Fill");
    //            newMaterial.Color = new Color(255, 0, 0);
    //            newMaterial.AppearanceAssetId = new ElementId(BuiltInParameter.MATERIAL_ASSET_ID_SOLID_FILL);

    //            // Assign the new material to the element
    //            element.LookupParameter("Material").Set(newMaterial);
    //        }
    //    }
    //}
}
