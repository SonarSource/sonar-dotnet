﻿using System.Threading.Tasks;

// Repro for https://github.com/SonarSource/sonar-dotnet/issues/6779
class Repro_FN_6779
{
    class SomeService
    {
        public ValueTask DoThing1() => ValueTask.CompletedTask;
        public ValueTask DoThing2() => ValueTask.CompletedTask;
    }

    class Consumer
    {
        public async Task ConsumeTasks()
        {
            var service = new SomeService();

            // The reason seems to be that 'GetLeftMostIdentifier' uses 'service' instead of 'service.DoThing1' as the identifier.
            // Invocation -> SimpleMember -> Invocation -> SimpleMember
            var thing1 = service.DoThing1().AsTask(); // Noncompliant FP
            var thing2 = service.DoThing2().AsTask();
//                       ^^^^^^^ Secondary
        }
    }
}
