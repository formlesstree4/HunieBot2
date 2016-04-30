using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace HunieBot.DiceRoll
{

    /// <summary>
    ///     Specifies additional processing options when a dice roll is being parsed.
    /// </summary>
    public enum DiceExpressionOptions
    {

        /// <summary>
        ///     No additional options are applied to the dice strong.
        /// </summary>
        None,

        /// <summary>
        ///     Attempts to simplify the dice string.
        /// </summary>
        SimplifyStringValue

    }

    /// <summary>
    ///     I found this somewhere on the Internet. It's awesome. Whomever made this is a saint.
    /// </summary>
    /// <remarks>
    ///     <expr> :=   <expr> + <expr>
    ///               | <expr> - <expr>
    ///               | [<number>]d(<number>|%)
    ///               | <number>
    ///     <number> := positive integer
    /// </remarks>
    public sealed class DiceExpression
    {

        /// <summary>
        ///     Gets a <see cref="DiceExpression"/> that always evaluates to zero.
        /// </summary>
        public static DiceExpression Zero { get; } = new DiceExpression("0");

        ///// <summary>
        /////     gets or sets the maximum number of dice that can be thrown at a single time.
        ///// </summary>
        ///// <remarks>
        /////     Take for example "1d20+5". <see cref="MaxDice"/> will limit the "1" portion to fit within the number specified.
        ///// </remarks>
        //public static int MaxDice { get; set; } = 10;


        private readonly Regex numberToken = new Regex("^[0-9]+$");
        private readonly Regex diceRollToken = new Regex("^([0-9]*)d([0-9]+|%)$");
        private List<KeyValuePair<long, IDiceExpressionNode>> nodes = new List<KeyValuePair<long, IDiceExpressionNode>>();



        /// <summary>
        ///     Gets a readonly collection of the parsed <see cref="IDiceExpressionNode"/>.
        /// </summary>
        /// <remarks>
        ///     The key in the returned <see cref="KeyValuePair{TKey, TValue}"/> indicates whether or not the <see cref="IDiceExpressionNode"/> associated with it is to be added or subtracted from the running total.
        /// </remarks>
        public IReadOnlyCollection<KeyValuePair<long, IDiceExpressionNode>> Expressions => nodes.AsReadOnly();



        /// <summary>
        ///     Creates a new instance of the <see cref="DiceExpression"/> class.
        /// </summary>
        /// <param name="expression">The string expression to parse.</param>
        public DiceExpression(string expression) : this(expression, DiceExpressionOptions.None) { }

        /// <summary>
        ///     Creates a new instance of the <see cref="DiceExpression"/> class.
        /// </summary>
        /// <param name="expression">The string expression to parse.</param>
        /// <param name="options"><see cref="DiceExpressionOptions"/></param>
        public DiceExpression(string expression, DiceExpressionOptions options)
        {
            // A well-formed dice expression's tokens will be either +, -, an integer, or XdY.
            var tokens = expression.Replace("+", " + ").Replace("-", " - ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Blank dice expressions end up being DiceExpression.Zero.
            if (!tokens.Any())
            {
                tokens = new[] { "0" };
            }

            // Since we parse tokens in operator-then-operand pairs, make sure the first token is an operand.
            if (tokens[0] != "+" && tokens[0] != "-")
            {
                tokens = (new[] { "+" }).Concat(tokens).ToArray();
            }

            // This is a precondition for the below parsing loop to make any sense.
            if (tokens.Length % 2 != 0)
            {
                throw new ArgumentException("The given dice expression was not in an expected format: even after normalization, it contained an odd number of tokens.");
            }

            // Parse operator-then-operand pairs into nodes.
            for (long tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex += 2)
            {
                var token = tokens[tokenIndex];
                var nextToken = tokens[tokenIndex + 1];

                if (token != "+" && token != "-")
                {
                    throw new ArgumentException("The given dice expression was not in an expected format.");
                }
                long multiplier = token == "+" ? +1 : -1;

                if (numberToken.IsMatch(nextToken))
                {
                    nodes.Add(new KeyValuePair<long, IDiceExpressionNode>(multiplier, new NumberNode(long.Parse(nextToken))));
                }
                else if (diceRollToken.IsMatch(nextToken))
                {
                    var match = diceRollToken.Match(nextToken);
                    long numberOfDice = Math.Min(10, match.Groups[1].Value == string.Empty ? 1 : long.Parse(match.Groups[1].Value));
                    long diceType = Math.Min(100, match.Groups[2].Value == "%" ? 100 : long.Parse(match.Groups[2].Value));
                    nodes.Add(new KeyValuePair<long, IDiceExpressionNode>(multiplier, new DiceRollNode(numberOfDice, diceType)));
                }
                else
                {
                    throw new ArgumentException("The given dice expression was not in an expected format: the non-operand token was neither a number nor a dice-roll expression.");
                }
            }

            // Sort the nodes in an aesthetically-pleasing fashion.
            var diceRollNodes = nodes.Where(pair => pair.Value.GetType() == typeof(DiceRollNode))
                                          .OrderByDescending(node => node.Key)
                                          .ThenByDescending(node => ((DiceRollNode)node.Value).DiceType)
                                          .ThenByDescending(node => ((DiceRollNode)node.Value).NumberOfDice);
            var numberNodes = nodes.Where(pair => pair.Value.GetType() == typeof(NumberNode))
                                        .OrderByDescending(node => node.Key)
                                        .ThenByDescending(node => node.Value.Evaluate());

            // If desired, merge all number nodes together, and merge dice nodes of the same type together.
            if (options == DiceExpressionOptions.SimplifyStringValue)
            {
                long number = numberNodes.Sum(pair => pair.Key * pair.Value.Evaluate());
                var diceTypes = diceRollNodes.Select(node => ((DiceRollNode)node.Value).DiceType).Distinct();
                var normalizedDiceRollNodes = from type in diceTypes
                                              let numDiceOfThisType = diceRollNodes.Where(node => ((DiceRollNode)node.Value).DiceType == type).Sum(node => node.Key * ((DiceRollNode)node.Value).NumberOfDice)
                                              where numDiceOfThisType != 0
                                              let multiplicand = numDiceOfThisType > 0 ? +1 : -1
                                              let absNumDice = Math.Abs(numDiceOfThisType)
                                              orderby multiplicand descending
                                              orderby type descending
                                              select new KeyValuePair<long, IDiceExpressionNode>(multiplicand, new DiceRollNode(absNumDice, type));

                nodes = (number == 0 ? normalizedDiceRollNodes
                                          : normalizedDiceRollNodes.Concat(new[] { new KeyValuePair<long, IDiceExpressionNode>(number > 0 ? +1 : -1, new NumberNode(number)) })).ToList();
            }
            // Otherwise, just put the dice-roll nodes first, then the number nodes.
            else
            {
                nodes = diceRollNodes.Concat(numberNodes).ToList();
            }
        }



        /// <summary>
        ///     Evaluates the <see cref="DiceExpression"/>.
        /// </summary>
        /// <returns><see cref="long"/></returns>
        /// <remarks>
        ///     Effectively, <see cref="Evaluate"/> rolls the dice.
        /// </remarks>
        public long Evaluate()
        {
            long result = 0;
            foreach (var pair in nodes)
            {
                result += pair.Key * pair.Value.Evaluate();
            }
            return result;
        }

        /// <summary>
        ///     Returns a calculated average of the <see cref="DiceExpression"/>
        /// </summary>
        /// <returns><see cref="decimal"/></returns>
        public decimal GetCalculatedAverage()
        {
            decimal result = 0;
            foreach (var pair in nodes)
            {
                result += pair.Key * pair.Value.GetCalculatedAverage();
            }
            return result;
        }

        /// <summary>
        ///     Returns the string equivalent of <see cref="DiceExpression"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = (nodes[0].Key == -1 ? "-" : string.Empty) + nodes[0].Value.ToString();
            foreach (var pair in nodes.Skip(1))
            {
                result += pair.Key == +1 ? " + " : " − "; // NOTE: unicode minus sign, not hyphen-minus '-'.
                result += pair.Value.ToString();
            }
            return result;
        }



        public interface IDiceExpressionNode
        {
            long Evaluate();
            decimal GetCalculatedAverage();
        }
        public sealed class NumberNode : IDiceExpressionNode
        {
            private long _theNumber;
            public NumberNode(long theNumber)
            {
                _theNumber = theNumber;
            }
            public long Evaluate()
            {
                return _theNumber;
            }

            public decimal GetCalculatedAverage()
            {
                return _theNumber;
            }
            public override string ToString()
            {
                return _theNumber.ToString();
            }
        }
        public sealed class DiceRollNode : IDiceExpressionNode
        {
            private static readonly Random roller = new SecureRandom();

            private long _numberOfDice;
            private long _diceType;
            public DiceRollNode(long numberOfDice, long diceType)
            {
                _numberOfDice = numberOfDice;
                _diceType = diceType;
            }

            public long Evaluate()
            {
                long total = 0;
                for (long i = 0; i < _numberOfDice; ++i)
                {
                    total += roller.Next(1, (int)_diceType + 1);
                }
                return total;
            }

            public decimal GetCalculatedAverage()
            {
                return _numberOfDice * ((_diceType + 1.0m) / 2.0m);
            }

            public override string ToString()
            {
                return string.Format("{0}d{1}", _numberOfDice, _diceType);
            }

            public long NumberOfDice
            {
                get { return _numberOfDice; }
            }
            public long DiceType
            {
                get { return _diceType; }
            }
        }

    }

}