using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmyAnt
{
	namespace Robot
	{

		class Program
		{
			static void Main(string[] args)
			{
				Student ret = new Student("testRobot.xml");
				ret.SetTestingCallBack(GetTester);
				string get = Console.ReadLine();

				while(get != "exit")
				{
					try
					{
						Console.WriteLine("The result of test is " + ret.Test(get) + ", The total score is " + ret.LastMaxScore);
					}
					catch(MissingMemberException)
					{
						Console.WriteLine("The test named \"" + get + "\" does not exist, please check");
					}
					finally
					{
						Console.WriteLine("Unknown error");
					}
					get = Console.ReadLine();
				}
			}
			static RobotTester.ITester GetTester(string testname)
			{
				var ret = new ArmyAnt.RobotTester.Tester();
				for(int i = 0; i < files.Length; i++)
				{
					ret.LoadQA(files[i]);
					if(ret.TestName == testname)
						return ret;
				}
				return null;
			}
			static bool CreateXml(string xmlName)
			{
				if(System.IO.File.Exists(xmlName))
					return false;
				System.IO.File.Create(xmlName);
				return true;
			}
			private static readonly string[] files = { "exam_ch_1.xml" };
		}
	}
}
