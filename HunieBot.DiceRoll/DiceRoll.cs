using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunieBot.DiceRoll
{

    /// <summary>
    ///     A bot that handles dice rolls.
    /// </summary>
    [HunieBot("Dice Roller")]
    public sealed class DiceRoll
    {
        private readonly int MaxIterations = 10;

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.Owner, "roll")]
        public async Task HandleDiceRoll(IHunieCommand command, ILogging logger)
        {
            long sum = 0;
            var diceMessageBuilder = new StringBuilder();
            var commandString = string.Join(" ", command.Parameters);
            var arrayOfRolls = commandString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            logger.Trace($"Full roll expression: {string.Join(" ", arrayOfRolls)}");
            foreach (var currentRoll in arrayOfRolls)
            {
                long currentSum = 0;
                DiceExpression current;
                var calculated = new List<long>();
                try
                {
                    current = new DiceExpression(currentRoll, DiceExpressionOptions.SimplifyStringValue);
                }
                catch (ArgumentException)
                {
                    return;
                }
                diceMessageBuilder.AppendLine(current.ToString());
                foreach (var expression in current.Expressions)
                {
                    var expValue = expression.Key * expression.Value.Evaluate();
                    diceMessageBuilder.AppendLine($"\t{expression.Value} = {expValue:N0}");
                    currentSum += expValue;
                }
                sum += currentSum;
                diceMessageBuilder.AppendLine($"Total = {currentSum:N0}");
            }
            if(arrayOfRolls.Length > 1) diceMessageBuilder.AppendLine($"Total: {sum:N0}");
            await command.Channel.SendMessage(diceMessageBuilder.ToString());
        }

    }

}