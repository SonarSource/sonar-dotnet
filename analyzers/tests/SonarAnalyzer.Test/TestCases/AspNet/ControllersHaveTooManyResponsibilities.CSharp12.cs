using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using TestInstrumentation.ResponsibilitySpecificServices;
using TestInstrumentation;

// Remark: secondary messages are asserted extensively, to ensure that grouping is done correctly and deterministically.

namespace TestInstrumentation
{
    public interface IServiceWithAnAPI { void Use(); }

    [ApiController]
    public class ApiController : ControllerBase { } // To shorten test cases declaration

    // These interfaces have been kept to simulate the actual ones, from MediatR, AutoMapper, etc.
    // For performance reasons, the analyzer ignores namespace and assembly, and only consider the name.
    // While that may comes with some false positives, the likelihood of that happening is very low.
    namespace WellKnownInterfacesExcluded
    {
        public interface ILogger<T> : IServiceWithAnAPI { }     // From Microsoft.Extensions.Logging
        public interface IMediator : IServiceWithAnAPI { }      // From MediatR
        public interface IMapper : IServiceWithAnAPI { }        // From AutoMapper
        public interface IConfiguration : IServiceWithAnAPI { } // From Microsoft.Extensions.Configuration
        public interface IBus : IServiceWithAnAPI { }           // From MassTransit
        public interface IMessageBus : IServiceWithAnAPI { }    // From NServiceBus
    }

    namespace WellKnownInterfacesNotExcluded
    {
        public interface IOption<T> : IServiceWithAnAPI { } // From Microsoft.Extensions.Options
    }

    namespace ResponsibilitySpecificServices
    {
        public interface IS1 : IServiceWithAnAPI { }
        public interface IS2 : IServiceWithAnAPI { }
        public interface IS3 : IServiceWithAnAPI { }
        public interface IS4 : IServiceWithAnAPI { }
        public interface IS5 : IServiceWithAnAPI { }
        public interface IS6 : IServiceWithAnAPI { }
        public interface IS7 : IServiceWithAnAPI { }
        public interface IS8 : IServiceWithAnAPI { }
        public interface IS9 : IServiceWithAnAPI { }
        public interface IS10 : IServiceWithAnAPI { }
    }
}

namespace WithInjectionViaPrimaryConstructors
{
    using TestInstrumentation.ResponsibilitySpecificServices;
    using TestInstrumentation.WellKnownInterfacesExcluded;
    using TestInstrumentation.WellKnownInterfacesNotExcluded;
    using TestInstrumentation;

    namespace AssertIssueLocationsAndMessage
    {
        // Noncompliant@+1 {{This controller has multiple responsibilities and could be split into 2 smaller units.}}
        public class TwoResponsibilities(IS1 s1, IS2 s2) : ApiController
        {
            public IActionResult A1() { s1.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
        }

        public class WithGenerics<T>(IS1 s1, IS2 s2) : ApiController // Noncompliant
        //           ^^^^^^^^^^^^
        {
            public IActionResult GenericAction<U>() { s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            //                   ^^^^^^^^^^^^^
            public IActionResult NonGenericAction() { s1.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
            //                   ^^^^^^^^^^^^^^^^
        }

        public class @event<T>(IS1 s1, IS2 s2) : ApiController // Noncompliant
        //           ^^^^^^
        {
            public IActionResult @private() { s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            //                   ^^^^^^^^
            public IActionResult @public() { s1.Use(); return Ok(); }  // Secondary {{Belongs to responsibility #2.}}
            //                   ^^^^^^^
        }

        public class ThreeResponsibilities(IS1 s1, IS2 s2, IS3 s3) : ApiController // Noncompliant
        //           ^^^^^^^^^^^^^^^^^^^^^
        {
            public IActionResult A1() { s1.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A3() { s3.Use(); return Ok(); } // Secondary {{Belongs to responsibility #3.}}
        }
    }

    namespace WithIOptions
    {
        // Noncompliant@+1: 4 deps injected, all well-known
        public class WellKnownDepsController(
            ILogger<WellKnownDepsController> logger, IMediator mediator, IMapper mapper, IConfiguration configuration) : ApiController
        {
            public IActionResult A1() { logger.Use(); return Ok(); }        // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); return Ok(); }      // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A3() { mapper.Use(); return Ok(); }        // Secondary {{Belongs to responsibility #3.}}
            public IActionResult A4() { configuration.Use(); return Ok(); } // Secondary {{Belongs to responsibility #4.}}
        }

        // Noncompliant@+1: 4 different Option<T> injected, that are not excluded
        public class FourDifferentOptionDepsController(
            IOption<int> o1, IOption<string> o2, IOption<bool> o3, IOption<double> o4) : ApiController
        {
            public IActionResult A1() { o1.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { o2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A3() { o3.Use(); return Ok(); } // Secondary {{Belongs to responsibility #3.}}
            public IActionResult A4() { o4.Use(); return Ok(); } // Secondary {{Belongs to responsibility #4.}}
        }

        // Compliant@+1: 5 different Option<T> injected, used in couples to form a single responsibility
        public class FourDifferentOptionDepsUsedInCouplesController(
                   IOption<int> o1, IOption<string> o2, IOption<bool> o3, IOption<double> o4, IOption<int> o5) : ApiController
        {
            public IActionResult A1() { o1.Use(); o2.Use(); return Ok(); }
            public IActionResult A2() { o2.Use(); o3.Use(); return Ok(); }
            public IActionResult A3() { o3.Use(); o4.Use(); return Ok(); }
            public IActionResult A4() { o4.Use(); o5.Use(); return Ok(); }
        }

        // Noncompliant@+1: 3 Option<T> deps injected, the rest are well-known dependencies (used as well as unused)
        public class ThreeOptionDepsController(
            ILogger<ThreeOptionDepsController> logger, IMediator mediator, IMapper mapper, IConfiguration configuration,
            IOption<int> o1, IOption<string> o2, IOption<bool> o3) : ApiController
        {
            public IActionResult A1() { o1.Use(); logger.Use(); return Ok(); }        // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { o2.Use(); mediator.Use(); return Ok(); }      // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A3() { o3.Use(); configuration.Use(); return Ok(); } // Secondary {{Belongs to responsibility #3.}}
            public IActionResult A4() { logger.Use(); return Ok(); }                  // Secondary {{Belongs to responsibility #4.}}
        }
    }

    namespace TwoActions
    {
        // Noncompliant@+1: 2 specific deps injected, each used in a separate responsibility, plus well-known dependencies
        public class TwoSeparateResponsibilitiesPlusSharedWellKnown(
            ILogger<TwoSeparateResponsibilitiesPlusSharedWellKnown> logger, IMediator mediator, IMapper mapper, IConfiguration configuration, IBus bus, IMessageBus messageBus,
            IS1 s1, IS2 s2) : ApiController
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); bus.Use(); configuration.Use(); s1.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); bus.Use(); configuration.Use(); s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
        }

        // Noncompliant@+1: 4 specific deps injected, two for A1 and two for A2
        public class FourSpecificDepsTwoForA1AndTwoForA2(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ApiController
        {
            public IActionResult A1() { logger.Use(); s1.Use(); s2.Use(); return Ok(); }                 // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
        }

        // Compliant@+1: 4 specific deps injected, two for A1 and two for A2, in non-API controller derived from Controller
        public class FourSpecificDepsTwoForA1AndTwoForA2NonApiFromController(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2NonApiFromController> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : Controller
        {
            public void A1() { logger.Use(); s1.Use(); s2.Use(); }
            public void A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); }
        }

        // Compliant@+1: 4 specific deps injected, two for A1 and two for A2, in non-API controller derived from ControllerBase
        public class FourSpecificDepsTwoForA1AndTwoForA2NonApiFromControllerBase(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2NonApiFromControllerBase> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            public void A1() { logger.Use(); s1.Use(); s2.Use(); }
            public void A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); }
        }

        // Compliant@+1: 4 specific deps injected, two for A1 and two for A2, in an API controller marked as NonController
        [NonController] public class FourSpecificDepsTwoForA1AndTwoForA2NoController(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2NoController> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ApiController
        {
            public IActionResult A1() { logger.Use(); s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return Ok(); }
        }

        // Noncompliant@+1: 4 specific deps injected, two for A1 and two for A2, in a PoCo controller with controller suffix
        public class FourSpecificDepsTwoForA1AndTwoForA2PoCoController(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2PoCoController> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ApiController
        {
            public string A1() { logger.Use(); s1.Use(); s2.Use(); return "Ok"; }                 // Secondary {{Belongs to responsibility #1.}}
            public string A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return "Ok"; } // Secondary {{Belongs to responsibility #2.}}
        }

        // Compliant@+1: 4 specific deps injected, two for A1 and two for A2, in a PoCo controller without controller suffix
        public class PoCoControllerWithoutControllerSuffix(
            ILogger<PoCoControllerWithoutControllerSuffix> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4)
        {
            public string A1() { logger.Use(); s1.Use(); s2.Use(); return "Ok"; }
            public string A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return "Ok"; }
        }

        // Noncompliant@+1: 4 specific deps injected, two for A1 and two for A2, in a PoCo controller without controller suffix but with [Controller] attribute
        [Controller] public class PoCoControllerWithoutControllerSuffixWithControllerAttribute(
            ILogger<PoCoControllerWithoutControllerSuffixWithControllerAttribute> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ApiController
        {
            public string A1() { logger.Use(); s1.Use(); s2.Use(); return "Ok"; }                 // Secondary {{Belongs to responsibility #1.}}
            public string A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return "Ok"; } // Secondary {{Belongs to responsibility #2.}}
        }

        // Noncompliant@+1: 4 specific deps injected, two for A1 and two for A2, with responsibilities in a different order
        public class FourSpecificDepsTwoForA1AndTwoForA2DifferentOrderOfResponsibilities(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2DifferentOrderOfResponsibilities> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ApiController
        {
            public IActionResult A2() { logger.Use(); s1.Use(); s2.Use(); return Ok(); }                 // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A1() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
        }

        // Noncompliant@+1: 4 specific deps injected, two for A1 and two for A2, with dependencies used in a different order
        public class FourSpecificDepsTwoForA1AndTwoForA2DifferentOrderOfDependencies(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2DifferentOrderOfDependencies> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ApiController
        {
            public IActionResult A1() { logger.Use(); s2.Use(); s1.Use(); return Ok(); }                 // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
        }

        // Noncompliant@+1: 4 specific deps injected, three for A1 and one for A2
        public class FourSpecificDepsThreeForA1AndOneForA2(
            ILogger<FourSpecificDepsThreeForA1AndOneForA2> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS3 s4) : ApiController
        {
            public IActionResult A1() { logger.Use(); s1.Use(); s2.Use(); s3.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s4.Use(); return Ok(); }     // Secondary {{Belongs to responsibility #2.}}
        }

        // Noncompliant@+1: 4 specific deps injected, all for A1 and none for A2
        public class FourSpecificDepsFourForA1AndNoneForA2(
            ILogger<FourSpecificDepsFourForA1AndNoneForA2> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS3 s4) : ApiController
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); s2.Use(); s3.Use(); s4.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); return Ok(); }                                         // Secondary {{Belongs to responsibility #2.}}
        }

        // Compliant@+1: 4 specific deps injected, one in common between responsibility 1 and 2
        public class ThreeSpecificDepsOneInCommonBetweenA1AndA2(
            ILogger<ThreeSpecificDepsOneInCommonBetweenA1AndA2> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3) : ApiController
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); s3.Use(); return Ok(); }
        }
    }

    namespace ThreeActions
    {
        // Noncompliant@+1: 2 specific deps injected, each used in a separate responsibility
        public class ThreeResponsibilities(
            ILogger<ThreeResponsibilities> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2) : ApiController
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A3() { mapper.Use(); return Ok(); }                           // Secondary {{Belongs to responsibility #3.}}
        }

        // Noncompliant@+1: 3 specific deps injected, each used in a separate responsibility, possibly multiple times
        public class UpToThreeSpecificDepsController(
            ILogger<UpToThreeSpecificDepsController> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3) : ApiController
        {
            public IActionResult A1() { logger.Use(); s1.Use(); s1.Use(); return Ok(); }                           // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { logger.Use(); mapper.Use(); s2.Use(); s2.Use(); return Ok(); }             // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A3() { mediator.Use(); mapper.Use(); s3.Use(); s3.Use(); s3.Use(); return Ok(); } // Secondary {{Belongs to responsibility #3.}}
        }

        // Noncompliant@+1: 3 specific deps injected, each used in a separate responsibility
        public class ThreeResponsibilities2(
            ILogger<ThreeResponsibilities2> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3) : ApiController
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A3() { s3.Use(); return Ok(); }                               // Secondary {{Belongs to responsibility #3.}}
        }

        // Noncompliant@+1: 3 specific deps injected, forming a chain and an isolated action
        public class ThreeSpecificDepsFormingAChainAndAnIsolatedAction(
            ILogger<ThreeSpecificDepsFormingAChainAndAnIsolatedAction> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3) : ApiController
        {
            // Chain: A1, A2 with s1 and s2
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); return Ok(); }           // Secondary {{Belongs to responsibility #1.}}
            // Isolated: A3 with s3
            public IActionResult A3() { s3.Use(); return Ok(); }                                         // Secondary {{Belongs to responsibility #2.}}
        }

        // Noncompliant@+1: 4 specific deps injected, two for A1, one for A2, and one for A3
        public class FourSpecificDepsTwoForA1OneForA2AndOneForA3(
            ILogger<FourSpecificDepsTwoForA1OneForA2AndOneForA3> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ApiController
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s3.Use(); return Ok(); }           // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A3() { s4.Use(); return Ok(); }                                         // Secondary {{Belongs to responsibility #3.}}
        }

        // Noncompliant@+1: 4 specific deps injected, one for A1, one for A2, one for A3, one unused
        public class FourSpecificDepsOneForA1OneForA2OneForA3OneUnused(
            ILogger<FourSpecificDepsOneForA1OneForA2OneForA3OneUnused> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ApiController
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A3() { s3.Use(); return Ok(); }                               // Secondary {{Belongs to responsibility #3.}}
        }

        // Compliant@+1: 4 specific deps injected, forming a single 3-cycle
        public class FourSpecificDepsFormingACycle(
            ILogger<FourSpecificDepsFormingACycle> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ApiController
        {
            // Cycle: A1, A2, A3
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); s3.Use(); return Ok(); }
            public IActionResult A3() { s3.Use(); s4.Use(); s1.Use(); return Ok(); }
        }
    }

    namespace SixActions
    {
        // Noncompliant@+1: 6 specific deps injected, forming 2 disconnected 3-cycles
        public class FourSpecificDepsFormingTwoDisconnectedCycles(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ApiController
        {
            // Cycle 1: A1, A2, A3
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { s2.Use(); s3.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A3() { s3.Use(); s1.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            // Cycle 2: A4, A5, A6 (disconnected from cycle 1)
            public IActionResult A4() { s4.Use(); s5.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A5() { s5.Use(); s6.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A6() { s6.Use(); s4.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
        }

        // Compliant@+1: 5 specific deps injected, forming 2 connected 3-cycles
        public class FourSpecificDepsFormingTwoConnectedCycles(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5) : ApiController
        {
            // Cycle 1: A1, A2, A3
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { s2.Use(); s3.Use(); return Ok(); }
            public IActionResult A3() { s3.Use(); s1.Use(); return Ok(); }
            // Cycle 2: A4, A5, A6 (connected to cycle 1 via s1)
            public IActionResult A4() { s1.Use(); s4.Use(); return Ok(); }
            public IActionResult A5() { s4.Use(); s5.Use(); return Ok(); }
            public IActionResult A6() { s5.Use(); s1.Use(); return Ok(); }
        }

        // Compliant@+1: 4 specific deps injected, forming 2 3-cycles, connected by two dependencies (s1 and s2)
        public class FourSpecificDepsFormingTwoConnectedCycles2(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ApiController
        {
            // Cycle 1: A1, A2, A3
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { s2.Use(); s3.Use(); return Ok(); }
            public IActionResult A3() { s3.Use(); s1.Use(); return Ok(); }
            // Cycle 2: A4, A5, A6
            public IActionResult A4() { s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A5() { s2.Use(); s4.Use(); return Ok(); }
            public IActionResult A6() { s4.Use(); s1.Use(); return Ok(); }
        }

        // Compliant@+1: 4 specific deps injected, forming 2 3-cycles, connected by action invocations
        public class FourSpecificDepsFormingTwoConnectedCycles3(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ApiController
        {
            // Cycle 1: A1, A2, A3
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { s2.Use(); s3.Use(); return Ok(); }
            public IActionResult A3() { s3.Use(); s1.Use(); return Ok(); }
            // Cycle 2: A4, A5, A6, connected to cycle 1 via A1 invocation
            public IActionResult A4() { A1(); s4.Use(); return Ok(); }
            public IActionResult A5() { s5.Use(); s6.Use(); return Ok(); }
            public IActionResult A6() { s6.Use(); s4.Use(); return Ok(); }
        }

        // Noncompliant@+1: 6 specific deps injected, forming 3 disconnected 2-cycles
        public class FourSpecificDepsFormingThreeDisconnectedCycles(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ApiController
        {
            // Cycle 1: A1, A2
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { s2.Use(); s1.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            // Cycle 2: A3, A4
            public IActionResult A3() { s3.Use(); s4.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A4() { s4.Use(); s3.Use(); return Ok(); } // Secondary {{Belongs to responsibility #2.}}
            // Cycle 3: A5, A6
            public IActionResult A5() { s5.Use(); s6.Use(); return Ok(); } // Secondary {{Belongs to responsibility #3.}}
            public IActionResult A6() { s6.Use(); s5.Use(); return Ok(); } // Secondary {{Belongs to responsibility #3.}}
        }

        // Noncompliant@+1: 6 specific deps injected, forming 2 connected 2-cycles and 1 disconnected 2-cycle
        public class FourSpecificDepsFormingTwoConnectedCyclesAndOneDisconnectedCycle(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ApiController
        {
            // Cycle 1: A1, A2
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); }       // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A2() { s2.Use(); s1.Use(); return Ok(); }       // Secondary {{Belongs to responsibility #1.}}
            // Cycle 2: A3, A4, connected to cycle 1 via A1 invocation
            public IActionResult A3() { A1(); s3.Use(); s4.Use(); return Ok(); } // Secondary {{Belongs to responsibility #1.}}
            public IActionResult A4() { s4.Use(); s3.Use(); return Ok(); }       // Secondary {{Belongs to responsibility #1.}}
            // Cycle 3: A5, A6
            public IActionResult A5() { s5.Use(); s6.Use(); return Ok(); }       // Secondary {{Belongs to responsibility #2.}}
            public IActionResult A6() { s6.Use(); s5.Use(); return Ok(); }       // Secondary {{Belongs to responsibility #2.}}
        }

        // Compliant@+1: 6 specific deps injected, forming 3 connected 2-cycles
        public class FourSpecificDepsFormingThreeConnectedCycles(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ApiController
        {
            // Cycle 1: A1, A2
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { s2.Use(); s1.Use(); return Ok(); }
            // Cycle 2: A3, A4, connected to cycle 1 via A1 invocation
            public IActionResult A3() { A1(); s3.Use(); s4.Use(); return Ok(); }
            public IActionResult A4() { s4.Use(); s3.Use(); return Ok(); }
            // Cycle 3: A5, A6, connected to cycle 1 via A2 invocation
            public IActionResult A5() { A2(); s5.Use(); s6.Use(); return Ok(); }
            public IActionResult A6() { s6.Use(); s5.Use(); return Ok(); }
        }

        // Compliant@+1: 6 specific deps injected, forming 3 connected 2-cycles - transitivity of connection
        public class FourSpecificDepsFormingThreeConnectedCyclesTransitivity(
                       IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ApiController
        {
            // Cycle 1: A1, A2
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { s2.Use(); s1.Use(); return Ok(); }
            // Cycle 2: A3, A4, connected to cycle 1 via A1 invocation
            public IActionResult A3() { A1(); s3.Use(); s4.Use(); return Ok(); }
            public IActionResult A4() { s4.Use(); s3.Use(); return Ok(); }
            // Cycle 3: A5, A6, connected to cycle 1 via A2 invocation
            public IActionResult A5() { A3(); s5.Use(); s6.Use(); return Ok(); }
            public IActionResult A6() { s6.Use(); s5.Use(); return Ok(); }
        }
    }
}

namespace WithInjectionViaNormalConstructor
{
    using TestInstrumentation.ResponsibilitySpecificServices;
    using TestInstrumentation.WellKnownInterfacesExcluded;
    using TestInstrumentation.WellKnownInterfacesNotExcluded;
    using TestInstrumentation;

    public class WithFields : ApiController // Noncompliant
    {
        private readonly IS1 s1;
        private IS2 s2;

        public WithFields(IS1 s1, IS2 s2) { this.s1 = s1; this.s2 = s2; }

        public void A1() { s1.Use(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{Belongs to responsibility #2.}}
    }

    public class WithDifferentVisibilities : ApiController // Noncompliant
    {
        private IS1 s1;
        protected IS2 s2;
        internal IS3 s3;
        private protected IS4 s4;
        protected internal IS5 s5;
        public IS6 s6;

        public WithDifferentVisibilities(IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6)
        {
            this.s1 = s1; this.s2 = s2; this.s3 = s3; this.s4 = s4; this.s5 = s5; this.s6 = s6;
        }

        private void A1() { s1.Use(); s2.Use(); }            // Secondary {{Belongs to responsibility #1.}}
        protected void A2() { s2.Use(); s1.Use(); }          // Secondary {{Belongs to responsibility #1.}}
        internal void A3() { s3.Use(); s4.Use(); }           // Secondary {{Belongs to responsibility #2.}}
        private protected void A4() { s4.Use(); s3.Use(); }  // Secondary {{Belongs to responsibility #2.}}
        protected internal void A5() { s5.Use(); s6.Use(); } // Secondary {{Belongs to responsibility #3.}}
        public void A6() { s6.Use(); s5.Use(); }             // Secondary {{Belongs to responsibility #3.}}
    }

    public class WithStaticFields : ApiController // Noncompliant: static storage is irrelevant
    {
        private static IS1 s1;
        private static IS2 s2 = null;

        public WithStaticFields(IS1 s1, IS2 s2) { WithStaticFields.s1 = s1; WithStaticFields.s2 = s2; }

        static WithStaticFields() { s1 = S1.Instance; }

        public void A1() { s1.Use(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{Belongs to responsibility #2.}}

        class S1 : IS1 { public void Use() { } public static IS1 Instance => new S1(); }
    }

    public class WithAutoProperties : ApiController // Noncompliant
    {
        public IS1 S1 { get; }
        public IS2 S2 { get; set; }
        protected IS3 S3 { get; init; }

        public WithAutoProperties(IS1 s1, IS2 s2, IS3 s3) { S1 = s1; S2 = s2; S3 = s3; }

        public void A1() { S1.Use(); }           // Secondary {{Belongs to responsibility #1.}}
        public void A2() { S2.Use(); }           // Secondary {{Belongs to responsibility #2.}}
        public void A3() { S3.Use(); S2.Use(); } // Secondary {{Belongs to responsibility #2.}}
    }

    public class WithFieldBackedProperties : ApiController // Noncompliant
    {
        private IS1 _s1;
        private IS2 _s2;

        public IS1 S1 { get => _s1; }
        public IS2 S2 { get => _s2; init => _s2 = value; }

        public WithFieldBackedProperties(IS1 s1, IS2 s2) { _s1 = s1; _s2 = s2; }

        public void A1() { S1.Use(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { S2.Use(); } // Secondary {{Belongs to responsibility #2.}}
    }

    public class WithMixedStorageMechanismsAndPropertyDependency : ApiController // Compliant: single responsibility
    {
        // Property dependency: A3 -> S3 -> { _s3, _s1 }
        private IS1 _s1;
        private IS3 _s3;

        public IS2 S2 { get; set; }
        public IS3 S3 { get => _s3; set { _s3 = value; _s1 = default; } }

        public WithMixedStorageMechanismsAndPropertyDependency(IS1 s1, IS2 s2, IS3 s3) { _s1 = s1; S2 = s2; _s3 = s3; }

        public void A1() { _s1.Use(); S2.Use(); }
        public void A2() { S2.Use(); S3.Use(); }
        public void A3() { S3.Use(); }
    }

    public class WithMixedStorageMechanismsAndPropertyDependencyTransitivity : ApiController
    {
        // Property dependency transitivity: A4 -> S4 -> { _s4, S3 } -> { _s4, _s3, S2 }
        private IS1 _s1;
        private IS3 _s3;
        private IS4 _s4;

        public IS2 S2 { get; set; }
        public IS3 S3 { get => _s3; set { _s3 = value; S2 = default; } } // Also resets S2
        public IS4 S4 { get => _s4; set { _s4 = value; S3 = default; } } // Also resets S3

        public WithMixedStorageMechanismsAndPropertyDependencyTransitivity(IS1 s1, IS2 s2, IS3 s3, IS4 s4)
        { _s1 = s1; S2 = s2; _s3 = s3; _s4 = s4; }

        public void A1() { _s1.Use(); S2.Use(); }
        public void A2() { S2.Use(); S3.Use(); }
        public void A3() { S3.Use(); _s1.Use(); }
        public void A4() { S4.Use(); }
    }

    public class WithLambdaCapturingService : ApiController // Noncompliant: s1Provider and s2Provider explicitly wrap services
    {
        private readonly Func<IS1> s1Provider;
        private readonly Func<int, IS2> s2Provider;

        public WithLambdaCapturingService(IS1 s1, IS2 s2) { s1Provider = () => s1; s2Provider = x => s2; }

        public void A1() { s1Provider().Use(); }   // Secondary {{Belongs to responsibility #1.}}
        public void A2() { s2Provider(42).Use(); } // Secondary {{Belongs to responsibility #2.}}
    }

    public class WithServiceWrappersInjection : ApiController // Noncompliant FP: no way to know whether s2Invoker wraps a service
    {
        private readonly Func<IS1> s1Provider;
        private readonly Action s2Invoker;

        public WithServiceWrappersInjection(Func<IS1> s1Provider, Action s2Invoker)
        {
            this.s1Provider = s1Provider;
            this.s2Invoker = s2Invoker;
        }

        public void A1() { s1Provider().Use(); s2Invoker(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { s2Invoker(); }                     // Secondary {{Belongs to responsibility #2.}}
    }

    public class WithNonPublicConstructor : ApiController // Noncompliant: ctor visibility is irrelevant
    {
        private readonly IS1 s1;
        protected IS2 S2 { get; init; }

        private WithNonPublicConstructor(IS1 s1, IS2 s2) { this.s1 = s1; this.S2 = s2; }

        public void A1() { s1.Use(); s1.Use(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { S2.Use(); }           // Secondary {{Belongs to responsibility #2.}}
    }

    public class WithCtorNotInitializingInjectedServices : ApiController // Noncompliant: initialization is irrelevant
    {
        private readonly IS1 s1;
        internal IS2 s2;

        public WithCtorNotInitializingInjectedServices(IS1 s1, IS2 s2) { }

        public void A1() { s1.Use(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{Belongs to responsibility #2.}}
    }

    public class WithServicesNotInjectedAtAll : ApiController // Noncompliant: ctor injection is irrelevant
    {
        private IS1 s1;
        private IS2 s2;

        public WithServicesNotInjectedAtAll() { }

        public void A1() { s1.Use(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{Belongs to responsibility #2.}}
    }

    public class WithServicesInitializedWithServiceProvider : ApiController // Noncompliant: service locator pattern is irrelevant
    {
        private readonly IS1 s1;
        private readonly IS2 s2;

        public WithServicesInitializedWithServiceProvider(IServiceProvider serviceProvider)
        {
            s1 = serviceProvider.GetRequiredService<IS1>();
            s2 = serviceProvider.GetRequiredService<IS2>();
        }

        public void A1() { s1.Use(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{Belongs to responsibility #2.}}
    }

    public class WithServicesInitializedWithSingletons : ApiController // Noncompliant: singleton pattern is irrelevant
    {
        private readonly IS1 s1 = S1.Instance;
        private readonly IS2 s2 = S2.Instance;

        public void A1() { s1.Use(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{Belongs to responsibility #2.}}

        class S1 : IS1 { public void Use() { } public static IS1 Instance => new S1(); }
        class S2 : IS2 { public void Use() { } public static IS2 Instance => new S2(); }
    }

    public class WithServicesInitializedWithMixedStrategies : ApiController // Noncompliant
    {
        private readonly IS1 s1;
        private readonly IS2 s2 = S2.Instance;
        private readonly IS3 s3;

        public WithServicesInitializedWithMixedStrategies(IS1 s1, IServiceProvider serviceProvider)
        {
            this.s1 = s1;
            s3 = serviceProvider.GetRequiredService<IS3>();
        }

        public void A1() { s1.Use(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{Belongs to responsibility #2.}}
        public void A3() { s3.Use(); } // Secondary {{Belongs to responsibility #3.}}

        class S2 : IS2 { public void Use() { } public static IS2 Instance => new S2(); }
    }

    public class WithAWellKnownInterfaceIncluded : ApiController // Noncompliant
    {
        private ILogger<WithAWellKnownInterfaceIncluded> Logger { get; }
        private readonly IS1 s1;
        private readonly IS2 s2 = S2.Instance;
        private readonly IS3 s3;

        public WithAWellKnownInterfaceIncluded(ILogger<WithAWellKnownInterfaceIncluded> logger, IS1 s1, IServiceProvider serviceProvider)
        {
            Logger = logger;
            this.s1 = s1;
            s3 = serviceProvider.GetRequiredService<IS3>();
        }

        public void A1() { Logger.Use(); s1.Use(); } // Secondary {{Belongs to responsibility #1.}}
        public void A2() { Logger.Use(); s2.Use(); } // Secondary {{Belongs to responsibility #2.}}
        public void A3() { Logger.Use(); s3.Use(); } // Secondary {{Belongs to responsibility #3.}}

        class S2 : IS2 { public void Use() { } public static IS2 Instance => new S2(); }
    }
}

public class WithUseInComplexBlocks : ApiController // Noncompliant
{
    IS1 s1; IS2 s2; IS3 s3; IS4 s4; IS5 s5; IS6 s6; IS7 s7; IS8 s8; IS9 s9; IS10 s10;

    public void If()     // Secondary {{Belongs to responsibility #2.}}
    {
        if (true) { s1.Use(); } else { if (false) { s2.Use(); } }
    }

    public void Switch() // Secondary {{Belongs to responsibility #2.}}
    {
        switch (0) { case 0: s2.Use(); break; case 1: s3.Use(); break; }
    }

    public void For()    // Secondary {{Belongs to responsibility #2.}}
    {
        for (int i = 0; i < 1; i++) { s1.Use(); s3.Use(); }
    }

    public void TryCatchFinally()      // Secondary {{Belongs to responsibility #3.}}
    {
        try { s4.Use(); } catch { s5.Use(); } finally { try { s6.Use(); } catch { s4.Use(); } }
    }

    public void Using()                // Secondary {{Belongs to responsibility #3.}}
    {
        using (new ADisposable()) { s5.Use(); s7.Use(); }
    }

    public void BlocksAndParentheses() // Secondary {{Belongs to responsibility #1.}}
    {
        { { ((s8)).Use(); } }
    }

    public void NestedLocalFunctions() // Secondary {{Belongs to responsibility #1.}}
    {
        void LocalFunction()
        {
            void NestedLocalFunction() { s8.Use(); }
            s9.Use();
        }

        LocalFunction();
        StaticLocalFunction(s10);

        static void StaticLocalFunction(IS10 s10) { s10.Use(); }
    }

    class ADisposable : IDisposable { public void Dispose() { } }
}

public class WithMethodsDependingOnEachOther : ApiController // Noncompliant
{
    IS1 s1; IS2 s2; IS3 s3; IS4 s4; IS5 s5; IS6 s6; IS7 s7;

    // Chain: A2 to A1
    void A1() { s1.Use(); }            // Secondary {{Belongs to responsibility #1.}}
    void A2() { s2.Use(); A1(); }      // Secondary {{Belongs to responsibility #1.}}
    void A3() { s2.Use(); }            // Secondary {{Belongs to responsibility #1.}}
    // 1-cycle A4
    void A4() { A4(); }                // Secondary {{Belongs to responsibility #7.}}
    // 2-cycle A5, A6
    void A5() { A6(); }                // Secondary {{Belongs to responsibility #8.}}
    void A6() { A5(); }                // Secondary {{Belongs to responsibility #8.}}
    // 3-cycle A7, A8, A9
    void A7() { A8(); }                // Secondary {{Belongs to responsibility #9.}}
    void A8() { A9(); }                // Secondary {{Belongs to responsibility #9.}}
    void A9() { A7(); }                // Secondary {{Belongs to responsibility #9.}}
    // 3-cycle A10, A11, A12 with A13 depending on A12 via s3
    void A10() { A11(); }              // Secondary {{Belongs to responsibility #2.}}
    void A11() { A12(); }              // Secondary {{Belongs to responsibility #2.}}
    void A12() { A10(); s3.Use(); }    // Secondary {{Belongs to responsibility #2.}}
    void A13() { s3.Use(); }           // Secondary {{Belongs to responsibility #2.}}
    // 3-cycle A14, A15, A16 with chain A18 -> A15 via s4
    void A14() { A15(); s4.Use(); }    // Secondary {{Belongs to responsibility #3.}}
    void A15() { A16(); }              // Secondary {{Belongs to responsibility #3.}}
    void A16() { A14(); }              // Secondary {{Belongs to responsibility #3.}}
    void A17() { s4.Use(); }           // Secondary {{Belongs to responsibility #3.}}
    // Independent method
    void A18() { }                     // Secondary {{Belongs to responsibility #4.}}
    // Independent method with its own service
    void A19() { s5.Use(); }           // Secondary {{Belongs to responsibility #5.}}
    // Two actions calling a third one
    void A20() { A22(); }              // Secondary {{Belongs to responsibility #6.}}
    void A21() { A22(); }              // Secondary {{Belongs to responsibility #6.}}
    void A22() { s6.Use(); s7.Use(); } // Secondary {{Belongs to responsibility #6.}}
}
