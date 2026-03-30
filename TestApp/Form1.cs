namespace TestApp
{
    public partial class Form1 : Form
    {
        public Form1(string[] args)
        {
            InitializeComponent();
            if (args.Length == 0)
            {
                label1.Text = "没有参数";
            }
            else if (args.Length >= 1)
            {
                label1.Text = args[0];
            }
        }
    }
}
