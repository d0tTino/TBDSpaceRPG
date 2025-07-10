using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.EditMode
{
    public class CrewMember
    {
        public string Id { get; }
        public CrewMember(string id) => Id = id;
    }

    public enum RelationshipType
    {
        Professional,
        Friendship
    }

    public class Relationship
    {
        public string CrewMember1Id;
        public string CrewMember2Id;
        public int Value;
        public RelationshipType Type;
    }

    public class CrewManager
    {
        private readonly List<CrewMember> _crew = new();
        public int Count => _crew.Count;

        public void AddCrew(CrewMember member) => _crew.Add(member);
        public bool RemoveCrew(CrewMember member) => _crew.Remove(member);
    }

    public class CrewTests
    {
        [Test]
        public void CrewManager_AddCrew_IncreasesCount()
        {
            var manager = new CrewManager();
            manager.AddCrew(new CrewMember("1"));
            Assert.AreEqual(1, manager.Count);
        }

        [Test]
        public void CrewManager_RemoveCrew_DecreasesCount()
        {
            var manager = new CrewManager();
            var member = new CrewMember("1");
            manager.AddCrew(member);
            manager.RemoveCrew(member);
            Assert.AreEqual(0, manager.Count);
        }

        [Test]
        public void Relationship_StoresDataCorrectly()
        {
            var rel = new Relationship
            {
                CrewMember1Id = "A",
                CrewMember2Id = "B",
                Value = 10,
                Type = RelationshipType.Professional
            };

            Assert.AreEqual("A", rel.CrewMember1Id);
            Assert.AreEqual("B", rel.CrewMember2Id);
            Assert.AreEqual(10, rel.Value);
            Assert.AreEqual(RelationshipType.Professional, rel.Type);
        }
    }
}
