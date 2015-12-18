using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ArmyAnt.RobotTester.Helper;

namespace ArmyAnt
{
	namespace RobotTester
	{
		namespace Helper
		{
			public delegate string AnswerWay(QuestionType type, string question, ushort pts);
			public delegate void AnswerRes(QuestionType type, string question, ushort pts, ushort gettedpts, string crightAns, string myAns);
			public delegate void TestEnd(string testname, uint totalGet, uint totalMax);
		}
		public enum QuestionType : byte
		{
			None = 0,
			Pinyin = 1,
			FillBlank_CH = 2
		}
		public interface ITester
		{
			/// <summary>
			/// 从xml文件载入题目及答案
			/// </summary>
			/// <returns></returns>
			bool LoadQA(string filename);
			bool StartExam(AnswerWay answay, AnswerRes ansnotify, TestEnd onEnd);
			bool StartExamAsync(AnswerWay answay, AnswerRes ansnotify, TestEnd onEnd);
			bool StopExamAsync();
        }
		internal class Question
		{
			internal Question(QuestionType type, string question, string answer,ushort pts)
			{
				this.type = type;
				this.question = question;
				this.answer = answer;
				this.pts = pts;
			}
			internal Question(byte type, string question, string answer, ushort pts)
			{
				this.type = (QuestionType)type;
				this.question = question;
				this.answer = answer;
				this.pts = pts;
			}
			internal QuestionType type = QuestionType.None;
			internal string question = null;
			internal string answer = null;
			internal ushort pts = 0;
		}
		public class Tester : ITester
		{
			public Tester(string filename = null)
			{
				if(filename != null)
					LoadQA(filename);
			}

			public bool LoadQA(string filename)
			{
				XmlDocument f = new XmlDocument();
				try
				{
					f.Load(filename);
				}
				catch(FileNotFoundException)
				{
					return false;
				}
				catch(XmlException)
				{
					return false;
				}
				testname = f.LastChild.Name;
				for(int i = 0; i < f.LastChild.ChildNodes.Count; i++)
				{
					var ret = new List<Question>();
					questions.Add(f.LastChild.ChildNodes[i].Name, ret);
					for(int j = 0; j < f.LastChild.ChildNodes[i].ChildNodes.Count; j++)
					{
						var res = f.LastChild.ChildNodes[i].ChildNodes[j].Attributes;
						ret.Add(new Question(Convert.ToByte(res["type"].Value), res["question"].Value, res["answer"].Value, Convert.ToUInt16(res["pts"].Value)));
					}
				}
				return true;
			}
			internal bool SaveQA(string filename)
			{
				XmlDocument f = new XmlDocument();
				f.LoadXml("<?xml version=\"1.0\" encoding=\"utf - 8\" ?> \n< " + TestName + ">\n</" + TestName + ">");
				for(int i = 0; i < questions.Count; i++)
				{
					f.LastChild.AppendChild(f.CreateElement(questions.Keys.ElementAt(i)));
					for(int j = 0; j < questions[questions.Keys.ElementAt(i)].Count; j++)
					{
						f.LastChild.LastChild.AppendChild(f.CreateElement("q"));
						f.LastChild.LastChild.LastChild.Attributes.Append(f.CreateAttribute("type"));
						f.LastChild.LastChild.LastChild.Attributes.Append(f.CreateAttribute("question"));
						f.LastChild.LastChild.LastChild.Attributes.Append(f.CreateAttribute("answer"));
						f.LastChild.LastChild.LastChild.Attributes.Append(f.CreateAttribute("pts"));
						f.LastChild.LastChild.LastChild.Attributes["type"].Value = ((int)questions[questions.Keys.ElementAt(i)][j].type).ToString();
						f.LastChild.LastChild.LastChild.Attributes["question"].Value = questions[questions.Keys.ElementAt(i)][j].question;
						f.LastChild.LastChild.LastChild.Attributes["answer"].Value = questions[questions.Keys.ElementAt(i)][j].answer;
						f.LastChild.LastChild.LastChild.Attributes["pts"].Value = questions[questions.Keys.ElementAt(i)][j].pts.ToString();
					}
				}
				return true;
			}
			public bool StartExam(AnswerWay answay, AnswerRes ansnotify, TestEnd onEnd)
			{
				bool re = false;
				return ExamExecute(answay, ansnotify, onEnd, ref re);
			}

			public bool StartExamAsync(AnswerWay answay, AnswerRes ansnotify, TestEnd onEnd)
			{
				ansEnd = false;
				ansRes = false;
				var res = new Thread(() => ExamExecute(answay, ansnotify, onEnd, ref ansEnd));
				res.Start();
				return true;
			}

			public bool StopExamAsync()
			{
				ansEnd = true;
				return ansRes;
			}
			public string TestName
			{
				get
				{
					return testname;
				}
			}
			protected bool ExamExecute(AnswerWay answay, AnswerRes ansnotify, TestEnd onEnd, ref bool isEnd)
			{
				if(isAnswering)
					return false;
				isAnswering = true;
				var titles = questions.Keys.ToArray();
				uint total = 0;
				uint max = 0;
				for(int i = 0;!ansEnd&& i < questions.Count; i++)
				{
					for(int j = 0; !ansEnd&&j < questions[titles[i]].Count; j++)
					{
						var curr = questions[titles[i]][j];
						var res = answay(curr.type, curr.question, curr.pts);
						if(res == curr.answer)
							total += curr.pts;
						max += curr.pts;
						ansnotify(curr.type, curr.question, curr.pts, (res == curr.answer) ? curr.pts : (ushort)0, curr.answer, res);
					}
				}
				if(ansEnd)
					return ansRes = false;
				onEnd(testname, total, max);
				isAnswering = false;
				return ansRes = true;
			}
			protected bool isAnswering = false;
			protected string testname = "";
			internal Dictionary<string, List<Question>> questions = new Dictionary<string, List<Question>>();
			private bool ansEnd = false;
			private bool ansRes = false;
		}
	}
}