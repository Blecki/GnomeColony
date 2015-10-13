using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpRuleEngine;

namespace Game.Input
{
    public enum CommandTargetType
    {
        None,
        Tile,
        Actor,
        Area,
    }

    public class PlayerCommand
    {
        public String Name;
        public int EnergyCost;
        public CommandTargetType TargetType;

        private CheckRuleBook CheckRules;
        private PerformRuleBook ProceduralRules;
        private World World;

        public PlayerCommand(World World)
        {
            this.World = World;

            CheckRules = new CheckRuleBook(World.GlobalRules.Rules) { ArgumentCount = 1 };
            ProceduralRules = new PerformRuleBook(World.GlobalRules.Rules) { ArgumentCount = 1 };
        }

        public CheckResult ConsiderCheck(Gem.PropertyBag Bag)
        {
            return CheckRules.Consider(Bag);
        }

        public PerformResult ConsiderPerform(Gem.PropertyBag Bag)
        {
            return ProceduralRules.Consider(Bag);
        }

        public PlayerCommand Perform(Func<Gem.PropertyBag, PerformResult> Rule)
        {
            var rule = new Rule<PerformResult>
            {
                BodyClause = RuleDelegateWrapper<PerformResult>.MakeWrapper(Rule),
            };
            ProceduralRules.AddRule(rule);
            return this;
        }

        public PlayerCommand Check(String RuleName, params String[] RuleArguments)
        {
            var rule = new Rule<CheckResult>
            {
                BodyClause = RuleDelegateWrapper<CheckResult>.MakeWrapper<Gem.PropertyBag>(
                (bag) =>
                {
                    return World.GlobalRules.ConsiderCheckRule(RuleName, RuleArguments.Select(a => bag.GetPropertyOrDefault(a)).ToArray());
                })
            };

            CheckRules.AddRule(rule);
            return this;
        }

        public PlayerCommand Perform(String RuleName, params String[] RuleArguments)
        {
            var rule = new Rule<PerformResult>
            {
                BodyClause = RuleDelegateWrapper<PerformResult>.MakeWrapper<Gem.PropertyBag>(
                (bag) =>
                {
                    World.GlobalRules.ConsiderPerformRule(RuleName, RuleArguments.Select(a => bag.GetPropertyOrDefault(a)).ToArray());
                    return PerformResult.Continue;
                })
            };
            ProceduralRules.AddRule(rule);
            return this;
        }

        public PlayerCommand AbideBy(String RuleName, params String[] RuleArguments)
        {
            var rule = new Rule<PerformResult>
            {
                BodyClause = RuleDelegateWrapper<PerformResult>.MakeWrapper<Gem.PropertyBag>(
                (bag) =>
                    World.GlobalRules.ConsiderPerformRule(RuleName, RuleArguments.Select(a => bag.GetPropertyOrDefault(a)).ToArray())
                    )
            };
            ProceduralRules.AddRule(rule);
            return this;
        }

    }
}
