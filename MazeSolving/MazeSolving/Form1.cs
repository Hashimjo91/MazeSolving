using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MazeSolving.Properties;

namespace MazeSolving
{
    public partial class Form1 : Form
    {
        public static int Speed = 301; // interval speed (timer2)
        public static int ClickCount1;
        private readonly List<Panel> _panl = new List<Panel>(); // the list of all panels in the maze
        public readonly List<Panel> CurrentPanelList = new List<Panel>();
        private readonly bool record = true;
        private Panel _currentpanel = new Panel(); //the moving panel in 'DFS'
        //to know all the moving points in the 'BFS' process
        private bool _dfs = true; // deciding which algorithm is used
        private string _direc = @"E:\Mazes\"; //Default path
        private Panel _endpoint = new Panel(); // the end point 'user define'
        private bool _filled; // determine if the maze is loaded or not
        private bool _finished; // to know if the process of solving is done
        private bool _generated;
        private List<Panel> _list1OfmovingpPanelsBfs = new List<Panel>(); // the list of moving panels in 'BFS'  
        // holding the path between current point and the start point 'DFS'
        private Queue<Panel> _listofmovingBfsPanels = new Queue<Panel>(); // the list of moving panels in 'BFS'  
        private int _mazeSize; //maze size (the hight and weidth of each panel )
        //private List<Point> _path = new List<Point>(); // No idea
        private Stack<Panel> _pathToCurrentPoint = new Stack<Panel>();
        //determine if the path from the moving point to the last intersect is recorded 'DFS' for backtracking process
        private Stack<Panel> _recorded = new Stack<Panel>(); // the path between the last intersect and the moving panel
        //private Stack<Point> _stack = new Stack<Point>(); // No idea
        private bool _starting; //to know if the user selected the set points button
        private Panel _startpoint = new Panel(); // the start point 'user define'
        private DateTime _startTime = DateTime.Now; //the time which the program started solving
        private int _stepCounter;
        public List<Panel> Intersections = new List<Panel>(); // hold each intersect that had been passed by 'BFS'
        public List<Panel> Lis2TofmovingpanPanelsBfs = new List<Panel>(); // the list of moving panels in 'BFS'  
        public bool MouseDownA; // to know a hold is on (timer2 ++ ) 
        public bool MouseDownB; // to know a hold is on (timer2 --)
        public bool Ongoing; // to know if the process of solving the maze is ongoing
        public Dictionary<Point, Panel> Panels = new Dictionary<Point, Panel>();
        public double TimeToSolve; //the time at each timer tick (timer2) 
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            progressBar1.Maximum = 1001; // Assigning the Maxvalue of the speed meter
            progressBar1.Minimum = 200; // Assigning the Minvalue of the speed meter
        }
        //painting the panel Green
        public void MakeGreen(Panel z)
        {
            z.ForeColor = Color.Green;
            z.BackColor = Color.Green;
            ClickCount1++;
            _startpoint = z;
        }
        //painting the panel Black
        public void ReturnPanel(Panel z)
        {
            z.ForeColor = Color.Black;
            z.BackColor = Color.Black;
        }
        //painting the panel Red
        public void MakeRed(Panel z)
        {
            z.ForeColor = Color.Red;
            z.BackColor = Color.Red;
            ClickCount1++;
            _endpoint = z;
        }
        //painting the panel Yellow
        public void MakeYellow(Panel z)
        {
            z.ForeColor = Color.Yellow;
            z.BackColor = Color.Yellow;
        }
        //painting the panel Gray
        public void MakeGray(Panel z)
        {
            z.ForeColor = Color.Gray;
            z.BackColor = Color.Gray;
        }
        //painting the panel White
        private void Remove_Panels(Panel z)
        {
            z.BackColor = Color.White;
            z.ForeColor = Color.White;
            // Controls.Remove(panels[new Point(k.X-534,k.Y)]);
        }
        // changing the style of the panel when the mouse is poiting at it 
        private void button__MouseHover(object sender, EventArgs e)
        {
            var z = (Panel) sender;
            z.BorderStyle = BorderStyle.Fixed3D;
        }
        // Reterning the style of the panel when the mouse leaves the panel
        private void button__MouseLeave(object sender, EventArgs e)
        {
            var z = (Panel) sender;
            z.BorderStyle = BorderStyle.FixedSingle;
        }
        //choosing the start point of the end point of the maze 
        // or to change the panel color between white and black (path/block)
        private void button__Click(object sender, EventArgs e)
        {
            var z = (Panel) sender;
            if (_starting && z.BackColor == Color.White)
            {
                if (ClickCount1 == 0)
                {
                    MakeGreen(z);
                    label6.Text = Resources.Form1_button__Click_Set_EndPoint_;
                }
                else if (ClickCount1 == 1)
                {
                    MakeRed(z);
                    label6.Text = Resources.Form1_button__Click_Select_Searching_Algorithm;
                }
            }
            else
            {
                if (z.BackColor == Color.White)
                    ReturnPanel(z);
                else
                    Remove_Panels(z);
            }
        }
        // when creating the maze after clicking the create button three buttons will appear 
        //  small large medium       
        private void button4_Click(object sender, EventArgs e)
        {
            button5.Visible = !button5.Visible;
            button6.Visible = !button6.Visible;
            button7.Visible = !button7.Visible;
            label6.Text = Resources.Form1_button4_Click_Select_Maze_Size;
        }
        // enable set,save,generate,reset_point and the rest_maze buttons
        // erasing the maze if exist
        // creating blank maze (black blocks) large size
        // size is determind by the hight and weidth of the panel (10)
        private void button5_Click(object sender, EventArgs e)
        {
            label6.Text = Resources.Form1_button5_Click_Creating_Maze___;
            Invalidate();
            _generated = false;
            button11.Enabled = true;
            button13.Enabled = true;
            button16.Enabled = true;
            button9.Enabled = true;
            if (_filled)
            {
                foreach (var item in _panl)
                {
                    Controls.Remove(item);
                }
                _filled = false;
                Panels.Clear();
                _panl.Clear();
            }
            panel1.SendToBack();
            for (var i = 0; i < panel1.Height; i += 10)
            {
                for (var j = 0; j < panel1.Width; j += 10)
                {
                    var block = new Panel
                    {
                        Location = (new Point(panel1.Location.X + i, panel1.Location.Y + j)),
                        BackColor = Color.Black,
                        ForeColor = Color.Black,
                        BorderStyle = BorderStyle.FixedSingle,
                        Size = new Size(10, 10)
                    };
                    block.Name = block.Location.ToString();
                    block.Click += button__Click;
                    block.MouseHover += button__MouseHover;
                    block.MouseLeave += button__MouseLeave;
                    Panels.Add(new Point(i, j), block);
                    Controls.Add(block);
                    _panl.Add(block);
                    block.BringToFront();
                }
            }
            _mazeSize = 10;
            _filled = true;
            label6.Text = Resources.Form1_button5_Click_Done_;
        }
        // enable set,save,generate,reset_point and the rest_maze buttons
        // erasing the maze if exist
        // creating blank maze (black blocks) medium size
        // size is determind by the hight and weidth of the panel (20)
        private void button6_Click(object sender, EventArgs e)
        {
            label6.Text = Resources.Form1_button5_Click_Creating_Maze___;
            Invalidate();
            _generated = false;
            button11.Enabled = true;
            button13.Enabled = true;
            button16.Enabled = true;
            button9.Enabled = true;
            if (_filled)
            {
                foreach (var item in _panl)
                {
                    Controls.Remove(item);
                }
                _filled = false;
                Panels.Clear();
                _panl.Clear();
            }
            panel1.SendToBack();
            //   block.Parent = panel1;
            for (var i = 0; i < panel1.Height; i += 20)
            {
                for (var j = 0; j < panel1.Width; j += 20)
                {
                    var block = new Panel
                    {
                        Location = (new Point(panel1.Location.X + i, panel1.Location.Y + j)),
                        BackColor = Color.Black,
                        ForeColor = Color.Black,
                        BorderStyle = BorderStyle.FixedSingle,
                        Size = new Size(20, 20)
                    };
                    block.Name = block.Location.ToString();
                    block.Click += button__Click;
                    block.MouseHover += button__MouseHover;
                    block.MouseLeave += button__MouseLeave;
                    Panels.Add(new Point(i, j), block);
                    Controls.Add(block);
                    _panl.Add(block);
                    block.BringToFront();
                }
            }
            _filled = true;
            _mazeSize = 020;
            label6.Text = Resources.Form1_button5_Click_Done_;
        }
        // enable set,save,generate,reset_point and the rest_maze buttons
        // erasing the maze if exist
        // creating blank maze (black blocks) small size
        // size is determind by the hight and weidth of the panel (30)
        private void button7_Click(object sender, EventArgs e)
        {
            label6.Text = Resources.Form1_button5_Click_Creating_Maze___;
            Invalidate();
            _generated = false;
            button11.Enabled = true;
            button13.Enabled = true;
            button16.Enabled = true;
            button9.Enabled = true;
            if (_filled)
            {
                foreach (var item in _panl)
                {
                    Controls.Remove(item);
                }
                _filled = false;
                Panels.Clear();
                _panl.Clear();
            }
            panel1.SendToBack();
            //   block.Parent = panel1;
            for (var i = 0; i < panel1.Height; i += 30)
            {
                for (var j = 0; j < panel1.Width; j += 30)
                {
                    var block = new Panel
                    {
                        Location = (new Point(panel1.Location.X + i, panel1.Location.Y + j)),
                        BackColor = Color.Black,
                        ForeColor = Color.Black,
                        Size = new Size(30, 30),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    block.Name = block.Location.ToString();
                    block.Click += button__Click;
                    block.MouseEnter += button__MouseHover;
                    block.MouseLeave += button__MouseLeave;
                    Panels.Add(new Point(i, j), block);
                    Controls.Add(block);
                    _panl.Add(block);
                    block.BringToFront();
                }
            }
            _filled = true;
            _mazeSize = 30;
            label6.Text = Resources.Form1_button5_Click_Done_;
        }

        // set points button is invoked here 
        private void button9_Click(object sender, EventArgs e)
        {
            _starting = !_starting;
            button8.Enabled = true;
            if (_starting)
            {
                label2.Text = Resources.Form1_button9_Click_ON;
                label6.Text = Resources.Form1_button9_Click_Set_Start_Point_;
            }
            else
            {
                label2.Text = Resources.Form1_button9_Click_OFF;
            }
        }
        // this event is envoked by the solve button 
        // 1- if the DFS algo is selected 
        //   a- addressing the start point as current point
        //   b- Enabling the timer (timer2)
        //   C- get all available pathes around this point
        // 2- if the BFS algo is selected 
        //   a- get all available pathes around the start point
        //   b- adding these points to the panel Queue
        //   c- Starting the timer (timer2)
        // 3- if the process of solving is already running  pause the timer which stop the process until pressed again  
        private void button8_Click(object sender, EventArgs e)
        {
            _startTime = DateTime.Now;

            if (!Ongoing)
            {
                label6.Text = Resources._7;
                if (_dfs)
                {
                    button14.Enabled = true;
                    button15.Enabled = true;
                    _currentpanel = _startpoint;
                    timer2.Enabled = !timer2.Enabled;

                    _pathToCurrentPoint = Checkpanels();
                }
                else
                {
                    _list1OfmovingpPanelsBfs = Bfs(_startpoint);
                    foreach (var item in _list1OfmovingpPanelsBfs)
                    {
                        _listofmovingBfsPanels.Enqueue(item);
                    }

                    timer2.Enabled = !timer2.Enabled;
                }
                Ongoing = true;
                button15.Enabled = true;
                button14.Enabled = true;
            }
            else
            {
                label6.Text = Resources.Form1_button8_Click_Pause_;
                timer2.Enabled = !timer2.Enabled;
            }
        }
        // moving the panel through the available pathes 
        public void IfDfs()
        {
            // try ; to prevent any null pointer exception 
            try
            {
                // if reached the end point successfully 
                // stop the timer and clear the path so the no back tracking is allowed after the finish
                if (_finished)
                {
                    _pathToCurrentPoint.Clear();

                    timer2.Enabled = false;
                    return; // return to close this method
                }
                //if reaching a deadend this will make the current point return to the last intersect (back tracking) and coloring the points gray to defer them from the rest of the panels (As visited) 
                if (_pathToCurrentPoint.Count != 0 && !_finished)
                {
                    var p = Checkpanels().ToList();
                    if (p.Count == 0 && !_finished)
                    {
                        MakeGray(_currentpanel);
                        _currentpanel = _recorded.Pop();
                        _stepCounter -= 2;
                        if (_recorded.Count == 0)
                        {
                            timer2.Enabled = false;
                        }
                    }
                    else
                    {
                        if (_pathToCurrentPoint.Count == 0)
                        {
                            timer2.Enabled = false;
                            return;
                        }
                        // adding the available pathes to the list 
                        // make the last visited point yellow
                        // adjusting the current point to hold the next point
                        // making the new current point green
                        foreach (var item in p)
                        {
                            _pathToCurrentPoint.Push(item);
                        }

                        if (record)
                        {
                            _recorded.Push(_currentpanel);
                        }
                        MakeYellow(_currentpanel);
                        _currentpanel = _pathToCurrentPoint.Pop();
                    }
                }
                MakeGreen(_currentpanel);
            }
            catch (Exception)
            {
                _finished = true;
                label3.Text = Resources.Form1_IfDfs_No_solution_available_;
            }
        }
        // checking if the maze isnt finished
        // and adding each available path to the currebt panels list
        public void IfBfs()
        {
            if (!_finished && _list1OfmovingpPanelsBfs.Count != 0)
                _list1OfmovingpPanelsBfs = MoveP(_list1OfmovingpPanelsBfs);
            // destributing the current point to each available path and adding them to the current point(s) list  
            else
            {
                _finished = true;
                label3.Text = Resources.Form1_IfDfs_No_solution_available_;
            }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            //desplaying the time spent to solve/currentTime 
            label5.Text = _stepCounter.ToString();
            label4.Text = DateTime.Now.Subtract(_startTime).Minutes + @":" + DateTime.Now.Subtract(_startTime).Seconds;
            if (MouseDownB) button14_Holding(); // calling the action when the + button is being held down 
            if (MouseDownA) button15_Holding(); // calling the action when the - button is being held down 
            progressBar1.Value = 1000 - timer2.Interval; // adjusting the speed meter
            if (timer2.Interval > 0 && timer2.Interval < 30)
            {
                // inform the user the speed is to high
                label3.Text = Resources.Form1_timer2_Tick_At_this_speed_the_result_may_not_be_accurate;
            }
            else
            {
                label3.Text = Resources.Form1_timer2_Tick_Solving___;
                //inform the user that the maze is currently under solving
            }
            //calling the proper algo each tick
            if (_dfs)
            {
                timer2.Enabled = false;
                IfDfs();
                _stepCounter++;
                if (!_finished) timer2.Enabled = true;
            }
            else
            {
                timer2.Enabled = false;
                IfBfs();
                _stepCounter++;
                if (!_finished) timer2.Enabled = true;
            }
        }
        //move the list of panels 'BFS'(current panels list)
        public List<Panel> MoveP(List<Panel> z1)
        {
            var q = new List<Panel>();
            try
            {
                foreach (var item in z1)
                {
                    MakeYellow(item);
                    q.AddRange(Bfs(item));
                }
            }
            catch (Exception)
            {
                label5.Text = _stepCounter.ToString();
            }
            return q;
        }
        // get all the available pathes from the current panel
        public Stack<Panel> Checkpanels()
        {
            var gchildren = new Stack<Panel>();

            var left = new Point(_currentpanel.Location.X - _mazeSize, _currentpanel.Location.Y);
            var up = new Point(_currentpanel.Location.X, _currentpanel.Location.Y - _mazeSize);
            var right = new Point(_currentpanel.Location.X + _mazeSize, _currentpanel.Location.Y);
            var down = new Point(_currentpanel.Location.X, _currentpanel.Location.Y + _mazeSize);

            var pLeft = (Panel) Controls[left.ToString()];

            var pUp = (Panel) Controls[up.ToString()];

            var pRight = (Panel) Controls[right.ToString()];

            var pDown = (Panel) Controls[down.ToString()];

            if (pLeft != null && pLeft.ForeColor == Color.White)
                gchildren.Push(pLeft);

            if (pUp != null && pUp.ForeColor == Color.White)
                gchildren.Push(pUp);

            if (pRight != null && pRight.ForeColor == Color.White)
                gchildren.Push(pRight);

            if (pDown != null && pDown.ForeColor == Color.White)
                gchildren.Push(pDown);

            try
            {
                if (pDown != null && (pLeft != null && (pUp != null && (pRight != null &&
                                                                        (pRight.BackColor == Color.Red ||
                                                                         pUp.BackColor == Color.Red ||
                                                                         pDown.BackColor == Color.Red ||
                                                                         pLeft.BackColor == Color.Red)))))
                {
                    _finished = true;
                    TimeToSolve = DateTime.Now.Millisecond - DateTime.Now.Millisecond;
                    timer2.Enabled = false;
                    MessageBox.Show(@"End Successfuly");
                    label3.Text = @"Done Successfully";
                    _pathToCurrentPoint.Clear();
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return gchildren;
        }
        // calling the save method and start it with a (0)parameter 
        private void button11_Click(object sender, EventArgs e)
        {
            SaveMaze(0);
        }
        //writing each panel coordenates and color to the file/path
        public void SaveMaze(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine(_mazeSize);
                foreach (var item in _panl)
                {
                    writer.WriteLine(item.Location.X + "," + item.Location.Y);
                    writer.WriteLine(item.ForeColor.Name);
                }
                writer.Close();
            }
        }
        // creating the file or the directory if not exist
        // recursivly call it self with a incremented parameter to avoid creating an existing path/file
        public void SaveMaze(int i)
        {
            try
            {
                if (!Directory.Exists(_direc))
                {
                    var fbd = new FolderBrowserDialog();
                    fbd.ShowDialog();
                    _direc = fbd.SelectedPath;
                    Directory.CreateDirectory(_direc + @"\Mazes");
                }
                var path = _direc + @"\Example " + i + @".mz";
                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                    SaveMaze(path);
                }
                else
                {
                    i++;
                    SaveMaze(i);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.GetBaseException().ToString());
            }
        }

        // load the file by reading first line as a maze size and then reading 2 lines 1-coordenate/name  2-color
        private void button12_Click(object sender, EventArgs e)
        {
            _panl.Clear();
            var lm = new OpenFileDialog
            {
                Multiselect = false,
                Filter = @"Maze (*.mz)|*.mz"
            };
            lm.ShowDialog();
            try
            {
                var mz = lm.FileName;
                if (mz == null) return;
                var st = new StreamReader(mz);
                _mazeSize = Convert.ToInt16(st.ReadLine());
                while (!st.EndOfStream)
                {
                    var position = st.ReadLine();
                    var pColor = st.ReadLine();
                                        if (position != null)
                    {
                        int posX = Convert.ToInt16(position.Substring(0, position.IndexOf(',')));
                        int posY = Convert.ToInt16(position.Substring(position.IndexOf(',') + 1));
                        var block = new Panel
                        {
                            Location = (new Point(posX, posY)),
                            BackColor = pColor == "Black" ? Color.Black : Color.White,
                            ForeColor = pColor == "Black" ? Color.Black : Color.White,
                            Size = new Size(_mazeSize, _mazeSize),
                            BorderStyle = BorderStyle.FixedSingle
                        };
                        block.Name = block.Location.ToString();
                        block.Click += button__Click;
                        block.MouseEnter += button__MouseHover;
                        block.MouseLeave += button__MouseLeave;
                        Panels.Add(new Point(posX, posY), block);
                        Controls.Add(block);
                        _panl.Add(block);
                        block.BringToFront();
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            button11.Enabled = true;
            button13.Enabled = true;
            button16.Enabled = true;
            button9.Enabled = true;
        }
        // call the generating function to create a random maze
        private void button13_Click(object sender, EventArgs e)
        {
            if (!_generated)
            {
                _generated = true;
                label6.Text = @"Generating...";
                button11.Enabled = true;
                button8.Enabled = true;
                button16.Enabled = true;

                var o = new Random();
                var val = o.Next(0, _panl.Count);
                GenerateMazeNr(_panl[val]);
                label6.Text = @"Done Generating";
                //foreach (var item in Panl)
                //{
                //    if (checkpanels().Count == 4)
                //        returnPanel(item);
                //}
                foreach (var item in _panl)
                {
                    if (item.ForeColor == Color.White)
                    {
                        Masking(item);
                    }
                }
            }
        }

        private void PreMasking(List<Panel> z)
        {
            foreach (var item in z)
            {
                if (CheckSurroundingG(item).Count == 2)
                {
                    ReturnPanel(item);
                    return;
                }
            }
        }

        // called at the generating method to get the next panel to make as white/path
        public List<Panel> CheckSurroundingG(Panel z)
        {
            var surrounding = new List<Panel>();
            try
            {
                var left = new Point(z.Location.X - _mazeSize, z.Location.Y);
                var up = new Point(z.Location.X, z.Location.Y - _mazeSize);
                var right = new Point(z.Location.X + _mazeSize, z.Location.Y);
                var down = new Point(z.Location.X, z.Location.Y + _mazeSize);

                Panel pLeft;
                if (left.X >= 0 && left.Y >= 0)
                    pLeft = (Panel) Controls[left.ToString()];
                else
                    pLeft = null;


                Panel pUp;
                if (up.X >= 0 && up.Y >= 0)
                    pUp = (Panel) Controls[up.ToString()];
                else
                    pUp = null;


                Panel pRight;
                if (right.X >= 0 && right.Y >= 0)
                    pRight = (Panel) Controls[right.ToString()];
                else
                    pRight = null;


                Panel pDown;
                if (down.X >= 0 && down.Y >= 0)
                    pDown = (Panel) Controls[down.ToString()];
                else
                    pDown = null;


                if (pLeft != null && pLeft.ForeColor == Color.Black)
                    surrounding.Add(pLeft);


                if (pUp != null && pUp.ForeColor == Color.Black)
                    surrounding.Add(pUp);

                if (pRight != null && pRight.ForeColor == Color.Black)
                    surrounding.Add(pRight);

                if (pDown != null && pDown.ForeColor == Color.Black)
                    surrounding.Add(pDown);
            }
            catch (Exception)
            {
                // ignored
            }

            return surrounding;
        }

        // when generating the maze we might have a non-connected paths which may lead to a closed or unfinished maze
        // so we start the another generating process at the first black cell or block 
        // as for the connectp method it remove a random cell around the generating point whitch may lead to connecting the mazes
        public void ConnectP(List<Panel> z)
        {
            var q1 = new Random();
            var q = q1.Next(0, z.Count);
            //foreach (var item in z)
            //    {
            if (z.Count != 0) Remove_Panels(z[q]);
            //      }
        }

        //returns the cells/panels that disconnect the created maze(if exist) and the under-generating mae
        public List<Panel> ConnectP(Panel z)
        {
            var surrounding = new List<Panel>();
            try
            {
                var left = new Point(z.Location.X - _mazeSize, z.Location.Y);
                var up = new Point(z.Location.X, z.Location.Y - _mazeSize);
                var right = new Point(z.Location.X + _mazeSize, z.Location.Y);
                var down = new Point(z.Location.X, z.Location.Y + _mazeSize);

                Panel pLeft;
                if (left.X >= 0 && left.Y >= 0)
                    pLeft = (Panel) Controls[left.ToString()];
                else
                    pLeft = null;


                Panel pUp;
                if (up.X >= 0 && up.Y >= 0)
                    pUp = (Panel) Controls[up.ToString()];
                else
                    pUp = null;


                Panel pRight;
                if (right.X >= 0 && right.Y >= 0)
                    pRight = (Panel) Controls[right.ToString()];
                else
                    pRight = null;


                Panel pDown;
                if (down.X >= 0 && down.Y >= 0)
                    pDown = (Panel) Controls[down.ToString()];
                else
                    pDown = null;


                if (pLeft != null && pLeft.ForeColor == Color.Gray)
                    surrounding.Add(pLeft);


                if (pUp != null && pUp.ForeColor == Color.Gray)
                    surrounding.Add(pUp);

                if (pRight != null && pRight.ForeColor == Color.Gray)
                    surrounding.Add(pRight);

                if (pDown != null && pDown.ForeColor == Color.Gray)
                    surrounding.Add(pDown);
            }
            catch (Exception)
            {
                // ignored
            }
            return surrounding;
        }

        // finds the first available black panel in the panel list(maze)
        public void GetBlackPanels()
        {
            foreach (var item in _panl)
            {
                if (item.ForeColor == Color.Black)
                {
                    ConnectP(ConnectP(item));
                    GenerateMazeNr(item);
                    break;
                }
            }
            foreach (var item in _panl)
            {
                if (item.ForeColor == Color.Gray)
                {
                    ReturnPanel(item);
                }
            }
        }

        // generate the maze using the DFS algorithm
        public void GenerateMazeNr(Panel z)
        {
            if (z.ForeColor == Color.Black || z.ForeColor == Color.Green)
            {
                Remove_Panels(z);
            }
            var rnd = new Random();

            var surrPnls = CheckSurroundingG(z);
            var rand = rnd.Next(0, surrPnls.Count);
            if (surrPnls.Count == 0)
            {
                GetBlackPanels();
                return;
            }
            var f = surrPnls[rand];
            surrPnls.RemoveAt(rand);
            foreach (var item in surrPnls)
            {
                MakeGray(item);
            }
            GenerateMazeNr(f);
        }

        // increasing the interval tick speed 
        private void button14_Holding()
        {
            if (timer2.Interval > 20)
                timer2.Interval -= 20;
        }

        // increasing the interval tick speed
        private void button14_Click(object sender, EventArgs e)
        {
            if (timer2.Interval > 10)
                timer2.Interval -= 10;
        }

        // decreasing the interval tick speed 
        private void button15_Holding()
        {
            if (timer2.Interval <= 999)
                timer2.Interval += 20;
        }

        // decreasing the interval tick speed 
        private void button15_Click(object sender, EventArgs e)
        {
            if (timer2.Interval <= 999)
                timer2.Interval += 10;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            // reset all the variable used in the maze gen/sol/pointSetting
            _stepCounter = 0;
            label5.Text = @"0";
            ClickCount1 = 0;
            Remove_Panels(_startpoint);
            Remove_Panels(_endpoint);
            _startpoint = null;
            _endpoint = null;
            _pathToCurrentPoint = new Stack<Panel>();
            Intersections = new List<Panel>();
            Ongoing = false;
            Speed = 300;
            _finished = false;
            _currentpanel = new Panel();
            _startpoint = new Panel();
            _endpoint = new Panel();
            _starting = false;
            _recorded = new Stack<Panel>();
            Lis2TofmovingpanPanelsBfs = new List<Panel>();
            _listofmovingBfsPanels = new Queue<Panel>();
            _list1OfmovingpPanelsBfs = new List<Panel>();
            Panels = new Dictionary<Point, Panel>();


            foreach (var item in _panl)
            {
                if (item.BackColor == Color.Gray || item.BackColor == Color.Yellow || item.BackColor == Color.Green ||
                    item.BackColor == Color.Red || item.BackColor == Color.Violet)
                {
                    Remove_Panels(item);
                }
            }
            _pathToCurrentPoint.Clear();
            _recorded.Clear();
            Lis2TofmovingpanPanelsBfs.Clear();
            _listofmovingBfsPanels.Clear();
            _list1OfmovingpPanelsBfs.Clear();
            CurrentPanelList.Clear();
            _currentpanel = null;

            label2.Text = _starting ? Resources.Form1_button9_Click_ON : Resources.Form1_button9_Click_OFF;
            label6.Text = @"All clear";
        }

        // returns the available paths according to the (panel z)
        public List<Panel> Bfs(Panel z)
        {
            var gchildren = new List<Panel>();

            var left = new Point(z.Location.X - _mazeSize, z.Location.Y);
            var up = new Point(z.Location.X, z.Location.Y - _mazeSize);
            var right = new Point(z.Location.X + _mazeSize, z.Location.Y);
            var down = new Point(z.Location.X, z.Location.Y + _mazeSize);

            var pLeft = (Panel) Controls[left.ToString()];

            var pUp = (Panel) Controls[up.ToString()];

            var pRight = (Panel) Controls[right.ToString()];

            var pDown = (Panel) Controls[down.ToString()];

            if (pLeft != null && pLeft.ForeColor == Color.White)
            {
                gchildren.Add(pLeft);
                MakeGreen(pLeft);
            }
            if (pUp != null && pUp.ForeColor == Color.White)
            {
                gchildren.Add(pUp);
                MakeGreen(pUp);
            }
            if (pRight != null && pRight.ForeColor == Color.White)
            {
                gchildren.Add(pRight);
                MakeGreen(pRight);
            }
            if (pDown != null && pDown.ForeColor == Color.White)
            {
                gchildren.Add(pDown);
                MakeGreen(pDown);
            }
            try
            {
                if (pDown != null &&
                    (pLeft != null &&
                     (pUp != null &&
                      (pRight != null &&
                       (pRight.BackColor == Color.Red || pUp.BackColor == Color.Red || pDown.BackColor == Color.Red ||
                        pLeft.BackColor == Color.Red)))))
                {
                    _finished = true;
                    _list1OfmovingpPanelsBfs.Remove(z);
                    label3.Text = @"Successfully Solved";
                    Btuf();
                    MessageBox.Show(@"End Successfuly");
                    timer2.Enabled = false;
                    _pathToCurrentPoint.Clear();
                }
            }
            catch (Exception)
            {
                // ignored
            }
            if (gchildren.Count > 1)
            {
                Intersections.AddRange(gchildren); //z.ForeColor = Color.YellowGreen; z.BackColor = Color.YellowGreen; 
            }
            if (gchildren.Count == 0 && !_finished)
            {
                timer2.Enabled = false;
                Bfsbt(z);
                timer2.Enabled = true;
            }
            return gchildren;
        }
        // after finish solving the maze back tracking each ongoing solving process
        public void Btuf()
        {
            foreach (var item in _list1OfmovingpPanelsBfs)
            {
                Bfsbt(item);
            }
        }

        // back tracking steps and coloring the path in gray 
        public void Bfsbt(Panel z)
        {
            var toGo = new Panel();
            var gchildren = new List<Panel>();

            var left = new Point(z.Location.X - _mazeSize, z.Location.Y);
            var up = new Point(z.Location.X, z.Location.Y - _mazeSize);
            var right = new Point(z.Location.X + _mazeSize, z.Location.Y);
            var down = new Point(z.Location.X, z.Location.Y + _mazeSize);

            var pLeft = (Panel) Controls[left.ToString()];

            var pUp = (Panel) Controls[up.ToString()];

            var pRight = (Panel) Controls[right.ToString()];

            var pDown = (Panel) Controls[down.ToString()];

            if (pLeft != null && pLeft.ForeColor == Color.Yellow)
            {
                gchildren.Add(pLeft);
                toGo = pLeft;
            }
            if (pUp != null && pUp.ForeColor == Color.Yellow)
            {
                gchildren.Add(pUp);
                toGo = pUp;
            }
            if (pRight != null && pRight.ForeColor == Color.Yellow)
            {
                gchildren.Add(pRight);
                toGo = pRight;
            }
            if (pDown != null && pDown.ForeColor == Color.Yellow)
            {
                gchildren.Add(pDown);
                toGo = pDown;
            }
            if (gchildren.Count == 1)
            {
                MakeGray(z);
                Bfsbt(toGo);
            }
            if (gchildren.Count == 0)
            {
                MakeGray(z);
            }
        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            _dfs = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            _dfs = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
        }

        private void Masking(Panel z)
        {
            var rightD = new Point(z.Location.X + _mazeSize, z.Location.Y + _mazeSize);
            var right = new Point(z.Location.X + _mazeSize, z.Location.Y);
            var down = new Point(z.Location.X, z.Location.Y + _mazeSize);

            try
            {
                var pRight = (Panel) Controls[right.ToString()];

                var pRightD = (Panel) Controls[rightD.ToString()];

                var pDown = (Panel) Controls[down.ToString()];


                if ((pRight != null && pRight.ForeColor == Color.White) &&
                    (pRightD != null && pRightD.ForeColor == Color.White) &&
                    (pDown != null && pDown.ForeColor == Color.White))
                {
                    var z1 = new List<Panel> {z, pRight, pRightD, pDown};
                    PreMasking(z1);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
        private void label7_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"You can select one point as a start or two point start(green) end(red) then click Solve! ");
        }
    }
}