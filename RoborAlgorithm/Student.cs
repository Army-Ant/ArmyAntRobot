using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ArmyAnt.RobotTester;

namespace ArmyAnt
{
	namespace Robot
	{
		public delegate ITester TestingCall(string testname);
		public class Student
		{
			public Student(string filename)
			{
				cfgfile = filename;
				var cfg = new XmlDocument();
				try
				{
					cfg.Load(filename);
				}
				catch(System.IO.FileNotFoundException)
				{
					cfg.LoadXml("<?xml version=\"1.0\" encoding=\"utf - 8\" ?><robot><self><name>Jason Robot</name><testname>exam_ch_1</testname></self><answers></answers></robot>");
					cfg.Save(filename);
				}
				this.cfg = cfg.LastChild;
			}
			public bool LoadConfigFromXml(string filename)
			{
				try
				{
					cfgfile = filename;
					var cfg = new XmlDocument();
					cfg.Load(filename);
					this.cfg = cfg.LastChild;
				}
				catch(System.IO.FileNotFoundException)
				{
					return false;
				}
				return true;
			}
			public bool SetTestingCallBack(TestingCall cb)
			{
				OnTest = cb;
				return true;
			}
			protected bool SetKnowledge(QuestionType type,string question,string answer)
			{
				if(cfg["answers"][type.ToString()] == null)
				{
					var ne = cfg.OwnerDocument.CreateElement(type.ToString());
					cfg["answers"].AppendChild(ne);
				}
				var src = cfg["answers"][type.ToString()][question];
				if(src == null)
				{
					var ne = cfg.OwnerDocument.CreateElement(question);
					cfg["answers"][type.ToString()].AppendChild(ne);
					src = cfg["answers"][type.ToString()][question];
				}
				bool found = false;
				for(int i = 0; i < src.ChildNodes.Count; i++)
				{
					if(src.ChildNodes[i].Attributes["answer"].Value == answer)
					{
						found = true;
						src.ChildNodes[i].Attributes["time"].Value = (Convert.ToUInt32(src.ChildNodes[i].Attributes["time"].Value) + 1).ToString();
						break;
					}
				}
				if(!found)
				{
					var ne = cfg.OwnerDocument.CreateElement("val");
					ne.SetAttribute("answer", answer);
					ne.SetAttribute("time", 1.ToString());
					src.AppendChild(ne);
				}
				return true;
			}
			protected string GetKnowledge(QuestionType type, string question)
			{
				if(cfg["answers"][type.ToString()] == null)
					return null;
				var src = cfg["answers"][type.ToString()][question];
				if(src == null || src.ChildNodes.Count <= 0)
					return null;
				string res = null;
				uint times = 0;
				for(int i = 0; i < src.ChildNodes.Count; i++)
				{
					uint nowtimes = Convert.ToUInt32(src.ChildNodes[i].Attributes["time"].Value);
					if(nowtimes > times)
					{
						times = nowtimes;
						res = src.ChildNodes[i].Attributes["answer"].Value;
					}
				}
				return res;
			}
			public uint Test(string testname = null)
			{
				if(OnTest == null)
					throw new MissingMethodException("The method \"OnTest\" does not exist, please call \"SetTestingCallBack\" before start the test");
				if(testname == null)
					testname = TestName;
				var ret = OnTest(testname);
				if(ret==null)
					throw new MissingMemberException("The method \"OnTest\" does not find the test file, please check the config");
				if(!ret.StartExam(OnAnswer, OnAnswerEnd, OnTestEnd))
					return 0;
				return latestTotalScore;
			}
			public string OnAnswer(QuestionType type, string question, ushort pts)
			{
				return GetKnowledge(type, question);
			}
			public void OnAnswerEnd(QuestionType type, string question, ushort pts, ushort gettedpts, string crightAns, string myAns)
			{
				SetKnowledge(type, question, crightAns);
			}
			public void OnTestEnd(string testname, uint totalGet, uint totalMax)
			{
				latestTestName = testname;
				latestTotalScore = totalGet;
				latestMaxScore = totalMax;
			}
			public event TestingCall OnTest = null;
			public string TestName
			{
				get
				{
					return cfg["self"]["testname"].InnerText;
				}
				set
				{
					cfg["self"]["testname"].InnerText = value;
				}
			}
			public string Name
			{
				get
				{
					return cfg["self"]["name"].InnerText;
				}
				set
				{
					cfg["self"]["name"].InnerText = value;
				}
			}
			public uint LastMaxScore
			{
				get
				{
					return latestMaxScore;
				}
			}
			public uint LastTotalScore
			{
				get
				{
					return latestTotalScore;
				}
			}
			~Student()
			{
				cfg.OwnerDocument.Save(cfgfile);
			}
			protected string cfgfile = "";
			protected XmlNode cfg = null;
			protected string latestTestName = "";
			protected uint latestTotalScore = 0;
			protected uint latestMaxScore = 0;
		}
	}
}
