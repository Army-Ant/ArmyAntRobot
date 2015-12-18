using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmyAnt.RobotTester
{
	public class TestMaker
	{
		TestMaker(string filename = null)
		{
			if(filename != null)
				GetTest(filename);
		}
		TestMaker(Tester tester)
		{
			GetTest(tester);
		}
		public bool GetTest(string filename)
		{
			tester = new Tester();
			if(!tester.LoadQA(filename))
			{
				tester = null;
				return false;
			}
			return true;
		}
		public bool GetTest(Tester tester)
		{
			this.tester = tester;
			return true;
		}
		public bool AddQuestion(string title, QuestionType type, string question, string answer, ushort pts)
		{
			if(tester == null)
				return false;
			if(tester.questions[title] == null)
				tester.questions.Add(title, new List<Question>());
			var src = tester.questions[title];
			for(int i = 0; i < src.Count; i++)
			{
				if(src[i].type == type && src[i].question == question)
				{
					src[i].answer = answer;
					src[i].pts = pts;
					return true;
				}
			}
			src.Add(new Question(type, question, answer, pts));
			return true;
		}



		private Tester tester = null;
	}
}
