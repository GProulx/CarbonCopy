﻿using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zinc.CarbonCopy.Replication.Cs
{
    class CsReplicateProvider : ReplicateProvider, IReplicateProvider
    {
        public CsReplicateProvider(Debugger debugger) : base(debugger) { }

        protected override Replicate CreateNullReplicate() { return new NullReplicate(); }
        protected override Replicate CreateClassReplicate() { return new ClassReplicate(); }
        protected override Replicate CreateListReplicate() { return new ListReplicate(); }
        protected override Replicate CreateArrayReplicate() { return new ArrayReplicate(); }
        protected override Replicate CreateDictionaryReplicate() { return new DictionaryReplicate(); }

        protected override List<Replicate> GetDictionaryMembers(string variableName)
        {
            var members = new List<Replicate>();

            var itemsCount = Int32.Parse(Debugger.GetExpression(String.Concat(variableName, ".Count()")).Value);

            for (int i = 0; i < itemsCount; i++)
            {
                KeyValuePairReplicate member = new KeyValuePairReplicate();

                member.Key = CreateReplicate(String.Concat(variableName, ".Keys[", i.ToString(), "]"));
                member.Value = CreateReplicate(String.Concat(variableName, ".Values[", i.ToString(), "]"));
                members.Add(member);
            }
            return members;
        }

        protected override List<Replicate> GetArrayMembers(string variableName)
        {
            var members = new List<Replicate>();

            var itemsCount = Int32.Parse(Debugger.GetExpression(String.Concat(variableName, ".Count()")).Value);

            for (int i = 0; i < itemsCount; i++)
            {
                members.Add(CreateReplicate(String.Concat(variableName, "[", i.ToString(), "]")));
            }

            return members;
        }

        protected override List<Replicate> GetListMembers(string variableName)
        {
            var members = new List<Replicate>();

            var itemsCount = Int32.Parse(Debugger.GetExpression(String.Concat(variableName, ".Count()")).Value);

            for (int i = 0; i < itemsCount; i++)
            {
                Replicate member = CreateReplicate(String.Concat(variableName, "[", i.ToString(), "]"));

                members.Add(member);
            }

            return members;
        }

        protected override List<Replicate> GetClassProperties(string variableName)
        {
            var properties = new List<Replicate>();

            string variableType = Debugger.GetExpression(String.Concat(variableName, ".GetType().FullName")).Value.Replace("\"", String.Empty).Replace("+", ".");

            EnvDTE.Expression expression = Debugger.GetExpression(String.Concat("(", variableType, ")", variableName)); //Need to cast it when list contains abstract type

            foreach (EnvDTE.Expression dataMember in expression.DataMembers)
            {
                var property = CreateReplicate(String.Concat(variableName, ".", dataMember.Name));

                if (property != null)
                {
                    properties.Add(property);
                }
            }

            if (properties.Count == 0)
            {
                properties = null;
            }

            return properties;
        }         
    }
}
