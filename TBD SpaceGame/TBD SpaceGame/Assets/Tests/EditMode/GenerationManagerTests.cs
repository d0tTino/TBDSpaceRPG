using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.EditMode
{
    public class CrewMember
    {
        public int Age { get; set; }
    }

    public class GenerationManager
    {
        public int CurrentGeneration { get; private set; }
        public event Action<int> OnGenerationAdvanced;
        public List<CrewMember> Crew { get; } = new();

        public void AdvanceGeneration()
        {
            CurrentGeneration++;
            foreach (var member in Crew)
            {
                member.Age++;
            }
            OnGenerationAdvanced?.Invoke(CurrentGeneration);
        }
    }

    public class GenerationManagerTests
    {
        [Test]
        public void AdvanceGeneration_IncrementsCrewAge()
        {
            var manager = new GenerationManager();
            var crew = new CrewMember { Age = 30 };
            manager.Crew.Add(crew);

            manager.AdvanceGeneration();

            Assert.AreEqual(31, crew.Age);
            Assert.AreEqual(1, manager.CurrentGeneration);
        }

        [Test]
        public void AdvanceGeneration_RaisesEvent()
        {
            var manager = new GenerationManager();
            int eventGen = -1;
            manager.OnGenerationAdvanced += gen => eventGen = gen;

            manager.AdvanceGeneration();

            Assert.AreEqual(1, eventGen);
        }

        [Test]
        public void AdvanceGeneration_MultipleCrew_AllAgesIncrement()
        {
            var manager = new GenerationManager();
            var crew1 = new CrewMember { Age = 25 };
            var crew2 = new CrewMember { Age = 40 };
            manager.Crew.Add(crew1);
            manager.Crew.Add(crew2);

            manager.AdvanceGeneration();

            Assert.AreEqual(26, crew1.Age);
            Assert.AreEqual(41, crew2.Age);
        }
    }
}
