using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using TestInstrumentation.ResponsibilitySpecificServices;
using TestInstrumentation.WellKnownInterfacesExcluded;
using TestInstrumentation;
using WithInjectionViaNormalConstructor;
using System.Runtime.CompilerServices;
using WithInjectionViaPrimaryConstructors.TwoActions;

// Remark: secondary messages are asserted extensively, to ensure that grouping is done correctly and deterministically.

namespace TestInstrumentation
{
    public interface IServiceWithAnAPI { void Use(); }

    public abstract class ServiceWithAnApi : IServiceWithAnAPI { public void Use() { } }

    // These interfaces have been kept to simulate the actual ones, from MediatR, AutoMapper, etc.
    // For performance reasons, the analyzer ignores namespace and assembly, and only consider the name.
    // While that may comes with some false positives, the likelihood of that happening is very low.
    namespace WellKnownInterfacesExcluded
    {
        public interface ILogger<T> : IServiceWithAnAPI { }         // From Microsoft.Extensions.Logging
        public interface IHttpClientFactory : IServiceWithAnAPI { } // From Microsoft.Extensions.Http
        public interface IMediator : IServiceWithAnAPI { }          // From MediatR
        public interface IMapper : IServiceWithAnAPI { }            // From AutoMapper
        public interface IConfiguration : IServiceWithAnAPI { }     // From Microsoft.Extensions.Configuration
        public interface IBus : IServiceWithAnAPI { }               // From MassTransit
        public interface IMessageBus : IServiceWithAnAPI { }        // From NServiceBus
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

        public class Class1: ServiceWithAnApi, IS1 { }
        public class Class2: ServiceWithAnApi, IS2 { }
        public class Class3: ServiceWithAnApi, IS3 { }
        public class Class4: ServiceWithAnApi, IS4 { }

        public class Struct1: IS1 { public void Use() { } }
        public class Struct2: IS2 { public void Use() { } }
        public class Struct3: IS3 { public void Use() { } }
        public class Struct4: IS4 { public void Use() { } }
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
        // Noncompliant@+2 {{This controller has multiple responsibilities and could be split into 2 smaller controllers.}}
        [ApiController]
        public class TwoResponsibilities(IS1 s1, IS2 s2) : ControllerBase
        {
            public IActionResult A1() { s1.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
        }

        [ApiController]
        public class WithGenerics<T>(IS1 s1, IS2 s2) : ControllerBase // Noncompliant
        //           ^^^^^^^^^^^^
        {
            public IActionResult GenericAction<U>() { s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            //                   ^^^^^^^^^^^^^
            public IActionResult NonGenericAction() { s1.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
            //                   ^^^^^^^^^^^^^^^^
        }

        [ApiController]
        public class @event<T>(IS1 s1, IS2 s2) : ControllerBase // Noncompliant
        //           ^^^^^^
        {
            public IActionResult @private() { s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            //                   ^^^^^^^^
            public IActionResult @public() { s1.Use(); return Ok(); }  // Secondary {{May belong to responsibility #2.}}
            //                   ^^^^^^^
        }

        [ApiController]
        public class ThreeResponsibilities(IS1 s1, IS2 s2, IS3 s3) : ControllerBase // Noncompliant
        //           ^^^^^^^^^^^^^^^^^^^^^
        {
            public IActionResult A1() { s1.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
            public IActionResult A3() { s3.Use(); return Ok(); } // Secondary {{May belong to responsibility #3.}}
        }
    }

    namespace WithIOptions
    {
        // Compliant@+2: 4 deps injected, all well-known => 4 singletons sets, merged into one
        [ApiController]
        public class WellKnownDepsController(
            ILogger<WellKnownDepsController> logger, IMediator mediator, IMapper mapper, IConfiguration configuration) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); return Ok(); }
            public IActionResult A2() { mediator.Use(); return Ok(); }
            public IActionResult A3() { mapper.Use(); return Ok(); }
            public IActionResult A4() { configuration.Use(); return Ok(); }
        }

        // Noncompliant@+2: 4 different Option<T> injected, that are not excluded
        [ApiController]
        public class FourDifferentOptionDepsController(
            IOption<int> o1, IOption<string> o2, IOption<bool> o3, IOption<double> o4) : ControllerBase
        {
            public IActionResult A1() { o1.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { o2.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
            public IActionResult A3() { o3.Use(); return Ok(); } // Secondary {{May belong to responsibility #3.}}
            public IActionResult A4() { o4.Use(); return Ok(); } // Secondary {{May belong to responsibility #4.}}
        }

        // Compliant@+2: 5 different Option<T> injected, used in couples to form a single responsibility
        [ApiController]
        public class FourDifferentOptionDepsUsedInCouplesController(
                   IOption<int> o1, IOption<string> o2, IOption<bool> o3, IOption<double> o4, IOption<int> o5) : ControllerBase
        {
            public IActionResult A1() { o1.Use(); o2.Use(); return Ok(); }
            public IActionResult A2() { o2.Use(); o3.Use(); return Ok(); }
            public IActionResult A3() { o3.Use(); o4.Use(); return Ok(); }
            public IActionResult A4() { o4.Use(); o5.Use(); return Ok(); }
        }

        // Noncompliant@+2: 3 Option<T> deps injected, the rest are well-known dependencies (used as well as unused)
        [ApiController]
        public class ThreeOptionDepsController(
            ILogger<ThreeOptionDepsController> logger, IMediator mediator, IMapper mapper, IConfiguration configuration,
            IOption<int> o1, IOption<string> o2, IOption<bool> o3) : ControllerBase
        {
            public IActionResult A1() { o1.Use(); logger.Use(); return Ok(); }        // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { o2.Use(); mediator.Use(); return Ok(); }      // Secondary {{May belong to responsibility #2.}}
            public IActionResult A3() { o3.Use(); configuration.Use(); return Ok(); } // Secondary {{May belong to responsibility #3.}}
            public IActionResult A4() { logger.Use(); return Ok(); }
        }
    }

    namespace TwoActions
    {
        // Noncompliant@+2: 2 specific deps injected, each used in a separate responsibility, plus well-known dependencies
        [ApiController]
        public class TwoSeparateResponsibilitiesPlusSharedWellKnown(
            ILogger<TwoSeparateResponsibilitiesPlusSharedWellKnown> logger, IMediator mediator, IMapper mapper, IConfiguration configuration, IBus bus, IMessageBus messageBus,
            IS1 s1, IS2 s2) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); bus.Use(); configuration.Use(); s1.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); bus.Use(); configuration.Use(); s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
        }

        // Noncompliant@+2: 4 specific deps injected, two for A1 and two for A2
        [ApiController]
        public class FourSpecificDepsTwoForA1AndTwoForA2(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); s1.Use(); s2.Use(); return Ok(); }                 // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
        }

        // Compliant@+1: 4 specific deps injected, two for A1 and two for A2, in non-API controller derived from Controller
        public class FourSpecificDepsTwoForA1AndTwoForA2NonApiFromController(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2NonApiFromController> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : Controller
        {
            public void A1() { logger.Use(); s1.Use(); s2.Use(); }
            public void A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); }
        }

        // Noncompliant@+1: 4 specific deps injected, two for A1 and two for A2, in non-API controller derived from ControllerBase
        public class FourSpecificDepsTwoForA1AndTwoForA2NonApiFromControllerBase(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2NonApiFromControllerBase> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            public void A1() { logger.Use(); s1.Use(); s2.Use(); }                 // Secondary {{May belong to responsibility #1.}}
            public void A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); } // Secondary {{May belong to responsibility #2.}}
        }

        // Compliant@+2: 4 specific deps injected, two for A1 and two for A2, in an API controller marked as NonController
        [ApiController]
        [NonController] public class FourSpecificDepsTwoForA1AndTwoForA2NoController(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2NoController> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return Ok(); }
        }

        // Noncompliant@+2: 4 specific deps injected, two for A1 and two for A2, in a PoCo controller with controller suffix
        [ApiController]
        public class FourSpecificDepsTwoForA1AndTwoForA2PoCoController(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2PoCoController> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            public string A1() { logger.Use(); s1.Use(); s2.Use(); return "Ok"; }                 // Secondary {{May belong to responsibility #1.}}
            public string A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return "Ok"; } // Secondary {{May belong to responsibility #2.}}
        }

        // Compliant@+1: 4 specific deps injected, two for A1 and two for A2, in a PoCo controller without controller suffix
        public class PoCoControllerWithoutControllerSuffix(
            ILogger<PoCoControllerWithoutControllerSuffix> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4)
        {
            public string A1() { logger.Use(); s1.Use(); s2.Use(); return "Ok"; }
            public string A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return "Ok"; }
        }

        // Noncompliant@+3: 4 specific deps injected, two for A1 and two for A2, in a PoCo controller without controller suffix but with [Controller] attribute
        [ApiController]
        [Controller]
        public class PoCoControllerWithoutControllerSuffixWithControllerAttribute(
            ILogger<PoCoControllerWithoutControllerSuffixWithControllerAttribute> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            public string A1() { logger.Use(); s1.Use(); s2.Use(); return "Ok"; }                 // Secondary {{May belong to responsibility #1.}}
            public string A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return "Ok"; } // Secondary {{May belong to responsibility #2.}}
        }

        // Noncompliant@+2: 4 specific deps injected, two for A1 and two for A2, with responsibilities in a different order
        [ApiController]
        public class FourSpecificDepsTwoForA1AndTwoForA2DifferentOrderOfResponsibilities(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2DifferentOrderOfResponsibilities> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            public IActionResult A2() { logger.Use(); s1.Use(); s2.Use(); return Ok(); }                 // Secondary {{May belong to responsibility #2.}}
            public IActionResult A1() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
        }

        // Noncompliant@+2: 4 specific deps injected, two for A1 and two for A2, with dependencies used in a different order
        [ApiController]
        public class FourSpecificDepsTwoForA1AndTwoForA2DifferentOrderOfDependencies(
            ILogger<FourSpecificDepsTwoForA1AndTwoForA2DifferentOrderOfDependencies> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); s2.Use(); s1.Use(); return Ok(); }                 // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s3.Use(); s4.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
        }

        // Noncompliant@+2: 4 specific deps injected, three for A1 and one for A2
        [ApiController]
        public class FourSpecificDepsThreeForA1AndOneForA2(
            ILogger<FourSpecificDepsThreeForA1AndOneForA2> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS3 s4) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); s1.Use(); s2.Use(); s3.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s4.Use(); return Ok(); }     // Secondary {{May belong to responsibility #2.}}
        }

        // Compliant@+2: 4 specific deps injected, all for A1 and none for A2
        [ApiController]
        public class FourSpecificDepsFourForA1AndNoneForA2(
            ILogger<FourSpecificDepsFourForA1AndNoneForA2> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS3 s4) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); s2.Use(); s3.Use(); s4.Use(); return Ok(); }
            public IActionResult A2() { mediator.Use(); mapper.Use(); return Ok(); }
        }

        // Compliant@+2: 4 specific deps injected, one in common between responsibility 1 and 2
        [ApiController]
        public class ThreeSpecificDepsOneInCommonBetweenA1AndA2(
            ILogger<ThreeSpecificDepsOneInCommonBetweenA1AndA2> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); s3.Use(); return Ok(); }
        }
    }

    namespace ThreeActions
    {
        // Noncompliant@+2: 2 specific deps injected, each used in a separate responsibility
        [ApiController]
        public class ThreeResponsibilities(
            ILogger<ThreeResponsibilities> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
            public IActionResult A3() { mapper.Use(); return Ok(); }
        }

        // Noncompliant@+2: 3 specific deps injected, each used in a separate responsibility, possibly multiple times
        [ApiController]
        public class UpToThreeSpecificDepsController(
            ILogger<UpToThreeSpecificDepsController> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); s1.Use(); s1.Use(); return Ok(); }                           // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { logger.Use(); mapper.Use(); s2.Use(); s2.Use(); return Ok(); }             // Secondary {{May belong to responsibility #2.}}
            public IActionResult A3() { mediator.Use(); mapper.Use(); s3.Use(); s3.Use(); s3.Use(); return Ok(); } // Secondary {{May belong to responsibility #3.}}
        }

        // Noncompliant@+2: 3 specific deps injected, each used in a separate responsibility
        [ApiController]
        public class ThreeResponsibilities2(
            ILogger<ThreeResponsibilities2> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
            public IActionResult A3() { s3.Use(); return Ok(); }                               // Secondary {{May belong to responsibility #3.}}
        }

        // Noncompliant@+2: 3 specific deps injected, forming a chain and an isolated action
        [ApiController]
        public class ThreeSpecificDepsFormingAChainAndAnIsolatedAction(
            ILogger<ThreeSpecificDepsFormingAChainAndAnIsolatedAction> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3) : ControllerBase
        {
            // Chain: A1, A2 with s1 and s2
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); return Ok(); }           // Secondary {{May belong to responsibility #1.}}
            // Isolated: A3 with s3
            public IActionResult A3() { s3.Use(); return Ok(); }                                         // Secondary {{May belong to responsibility #2.}}
        }

        // Noncompliant@+2: 4 specific deps injected, two for A1, one for A2, and one for A3
        [ApiController]
        public class FourSpecificDepsTwoForA1OneForA2AndOneForA3(
            ILogger<FourSpecificDepsTwoForA1OneForA2AndOneForA3> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s3.Use(); return Ok(); }           // Secondary {{May belong to responsibility #2.}}
            public IActionResult A3() { s4.Use(); return Ok(); }                                         // Secondary {{May belong to responsibility #3.}}
        }

        // Noncompliant@+2: 4 specific deps injected, one for A1, one for A2, one for A3, one unused
        [ApiController]
        public class FourSpecificDepsOneForA1OneForA2OneForA3OneUnused(
            ILogger<FourSpecificDepsOneForA1OneForA2OneForA3OneUnused> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
            public IActionResult A3() { s3.Use(); return Ok(); }                               // Secondary {{May belong to responsibility #3.}}
        }

        // Compliant@+2: 4 specific deps injected, forming a single 3-cycle
        [ApiController]
        public class FourSpecificDepsFormingACycle(
            ILogger<FourSpecificDepsFormingACycle> logger, IMediator mediator, IMapper mapper,
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
        {
            // Cycle: A1, A2, A3
            public IActionResult A1() { logger.Use(); mediator.Use(); s1.Use(); s2.Use(); return Ok(); }
            public IActionResult A2() { mediator.Use(); mapper.Use(); s2.Use(); s3.Use(); return Ok(); }
            public IActionResult A3() { s3.Use(); s4.Use(); s1.Use(); return Ok(); }
        }
    }

    namespace SixActions
    {
        // Noncompliant@+2: 6 specific deps injected, forming 2 disconnected 3-cycles
        [ApiController]
        public class FourSpecificDepsFormingTwoDisconnectedCycles(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ControllerBase
        {
            // Cycle 1: A1, A2, A3
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { s2.Use(); s3.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A3() { s3.Use(); s1.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            // Cycle 2: A4, A5, A6 (disconnected from cycle 1)
            public IActionResult A4() { s4.Use(); s5.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
            public IActionResult A5() { s5.Use(); s6.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
            public IActionResult A6() { s6.Use(); s4.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
        }

        // Compliant@+2: 5 specific deps injected, forming 2 connected 3-cycles
        [ApiController]
        public class FourSpecificDepsFormingTwoConnectedCycles(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5) : ControllerBase
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

        // Compliant@+2: 4 specific deps injected, forming 2 3-cycles, connected by two dependencies (s1 and s2)
        [ApiController]
        public class FourSpecificDepsFormingTwoConnectedCycles2(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4) : ControllerBase
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

        // Compliant@+2: 4 specific deps injected, forming 2 3-cycles, connected by action invocations
        [ApiController]
        public class FourSpecificDepsFormingTwoConnectedCycles3(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ControllerBase
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

        // Noncompliant@+2: 6 specific deps injected, forming 3 disconnected 2-cycles
        [ApiController]
        public class FourSpecificDepsFormingThreeDisconnectedCycles(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ControllerBase
        {
            // Cycle 1: A1, A2
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { s2.Use(); s1.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            // Cycle 2: A3, A4
            public IActionResult A3() { s3.Use(); s4.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
            public IActionResult A4() { s4.Use(); s3.Use(); return Ok(); } // Secondary {{May belong to responsibility #2.}}
            // Cycle 3: A5, A6
            public IActionResult A5() { s5.Use(); s6.Use(); return Ok(); } // Secondary {{May belong to responsibility #3.}}
            public IActionResult A6() { s6.Use(); s5.Use(); return Ok(); } // Secondary {{May belong to responsibility #3.}}
        }

        // Noncompliant@+2: 6 specific deps injected, forming 2 connected 2-cycles and 1 disconnected 2-cycle
        [ApiController]
        public class FourSpecificDepsFormingTwoConnectedCyclesAndOneDisconnectedCycle(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ControllerBase
        {
            // Cycle 1: A1, A2
            public IActionResult A1() { s1.Use(); s2.Use(); return Ok(); }       // Secondary {{May belong to responsibility #1.}}
            public IActionResult A2() { s2.Use(); s1.Use(); return Ok(); }       // Secondary {{May belong to responsibility #1.}}
            // Cycle 2: A3, A4, connected to cycle 1 via A1 invocation
            public IActionResult A3() { A1(); s3.Use(); s4.Use(); return Ok(); } // Secondary {{May belong to responsibility #1.}}
            public IActionResult A4() { s4.Use(); s3.Use(); return Ok(); }       // Secondary {{May belong to responsibility #1.}}
            // Cycle 3: A5, A6
            public IActionResult A5() { s5.Use(); s6.Use(); return Ok(); }       // Secondary {{May belong to responsibility #2.}}
            public IActionResult A6() { s6.Use(); s5.Use(); return Ok(); }       // Secondary {{May belong to responsibility #2.}}
        }

        // Compliant@+2: 6 specific deps injected, forming 3 connected 2-cycles
        [ApiController]
        public class FourSpecificDepsFormingThreeConnectedCycles(
            IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ControllerBase
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

        // Compliant@+2: 6 specific deps injected, forming 3 connected 2-cycles - transitivity of connection
        [ApiController]
        public class FourSpecificDepsFormingThreeConnectedCyclesTransitivity(
                       IS1 s1, IS2 s2, IS3 s3, IS4 s4, IS5 s5, IS6 s6) : ControllerBase
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

    [ApiController]
    public class WithFields : ControllerBase // Noncompliant
    {
        private readonly IS1 s1;
        private IS2 s2;

        public WithFields(IS1 s1, IS2 s2) { this.s1 = s1; this.s2 = s2; }

        public void A1() { s1.Use(); } // Secondary {{May belong to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #2.}}
    }

    [ApiController]
    public class WithDifferentVisibilities : ControllerBase // Noncompliant
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

        private void A1() { s1.Use(); s2.Use(); }            // Secondary {{May belong to responsibility #1.}}
        protected void A2() { s2.Use(); s1.Use(); }          // Secondary {{May belong to responsibility #1.}}
        internal void A3() { s3.Use(); s4.Use(); }           // Secondary {{May belong to responsibility #2.}}
        private protected void A4() { s4.Use(); s3.Use(); }  // Secondary {{May belong to responsibility #2.}}
        protected internal void A5() { s5.Use(); s6.Use(); } // Secondary {{May belong to responsibility #3.}}
        public void A6() { s6.Use(); s5.Use(); }             // Secondary {{May belong to responsibility #3.}}
    }

    [ApiController]
    public class WithStaticFields : ControllerBase // Compliant, we don't take into account static fields and methods.
    {
        private static IS1 s1;
        private static IS2 s2 = null;

        public WithStaticFields(IS1 s1, IS2 s2) { WithStaticFields.s1 = s1; WithStaticFields.s2 = s2; }

        static WithStaticFields() { s1 = S1.Instance; }

        public void A1() { s1.Use(); }
        public void A2() { s2.Use(); }

        class S1 : IS1 { public void Use() { } public static IS1 Instance => new S1(); }
    }

    [ApiController]
    public class WithAutoProperties : ControllerBase // Noncompliant
    {
        public IS1 S1 { get; }
        public IS2 S2 { get; set; }
        protected IS3 S3 { get; init; }
        private ILogger<WithAutoProperties> S4 { get; init; } // Well-known

        public WithAutoProperties(IS1 s1, IS2 s2, IS3 s3) { S1 = s1; S2 = s2; S3 = s3; }

        public void A1() { S1.Use(); }           // Secondary {{May belong to responsibility #1.}}
        public void A2() { S2.Use(); }           // Secondary {{May belong to responsibility #2.}}
        public void A3() { S3.Use(); S2.Use(); } // Secondary {{May belong to responsibility #2.}}
        public void A4() { S4.Use(); }           // Only well-known service used => Ignored
    }

    [ApiController]
    public class WithFieldBackedProperties : ControllerBase // Noncompliant
    {
        private IS1 _s1;
        private IS2 _s2;

        public IS1 S1 { get => _s1; }
        public IS2 S2 { get => _s2; init => _s2 = value; }

        public WithFieldBackedProperties(IS1 s1, IS2 s2) { _s1 = s1; _s2 = s2; }

        public void A1() { S1.Use(); } // Secondary {{May belong to responsibility #1.}}
        public void A2() { S2.Use(); } // Secondary {{May belong to responsibility #2.}}
    }

    [ApiController]
    public class WithMixedStorageMechanismsAndPropertyDependency : ControllerBase // Compliant: single responsibility
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

    [ApiController]
    public class WithMixedStorageMechanismsAndPropertyDependencyTransitivity : ControllerBase
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

    [ApiController]
    public class WithLambdaCapturingService : ControllerBase // Noncompliant: s1Provider and s2Provider explicitly wrap services
    {
        private readonly Func<IS1> s1Provider;
        private readonly Func<int, IS2> s2Provider;

        public WithLambdaCapturingService(IS1 s1, IS2 s2) { s1Provider = () => s1; s2Provider = x => s2; }

        public void A1() { s1Provider().Use(); }   // Secondary {{May belong to responsibility #1.}}
        public void A2() { s2Provider(42).Use(); } // Secondary {{May belong to responsibility #2.}}
    }

    [ApiController]
    public class WithNonPublicConstructor : ControllerBase // Noncompliant: ctor visibility is irrelevant
    {
        private readonly IS1 s1;
        protected IS2 S2 { get; init; }

        private WithNonPublicConstructor(IS1 s1, IS2 s2) { this.s1 = s1; this.S2 = s2; }

        public void A1() { s1.Use(); s1.Use(); } // Secondary {{May belong to responsibility #1.}}
        public void A2() { S2.Use(); }           // Secondary {{May belong to responsibility #2.}}
    }

    [ApiController]
    public class WithCtorNotInitializingInjectedServices : ControllerBase // Noncompliant: initialization is irrelevant
    {
        private readonly IS1 s1;
        internal IS2 s2;

        public WithCtorNotInitializingInjectedServices(IS1 s1, IS2 s2) { }

        public void A1() { s1.Use(); } // Secondary {{May belong to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #2.}}
    }

    [ApiController]
    public class WithServicesNotInjectedAtAll : ControllerBase // Noncompliant: ctor injection is irrelevant
    {
        private IS1 s1;
        private IS2 s2;

        public WithServicesNotInjectedAtAll() { }

        public void A1() { s1.Use(); } // Secondary {{May belong to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #2.}}
    }

    [ApiController]
    public class WithServicesInitializedWithServiceProvider : ControllerBase // Noncompliant: service locator pattern is irrelevant
    {
        private readonly IS1 s1;
        private readonly IS2 s2;

        public WithServicesInitializedWithServiceProvider(IServiceProvider serviceProvider)
        {
            s1 = serviceProvider.GetRequiredService<IS1>();
            s2 = serviceProvider.GetRequiredService<IS2>();
        }

        public void A1() { s1.Use(); } // Secondary {{May belong to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #2.}}
    }

    [ApiController]
    public class WithServicesInitializedWithSingletons : ControllerBase // Noncompliant: singleton pattern is irrelevant
    {
        private readonly IS1 s1 = S1.Instance;
        private readonly IS2 s2 = S2.Instance;

        public void A1() { s1.Use(); } // Secondary {{May belong to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #2.}}

        class S1 : IS1 { public void Use() { } public static IS1 Instance => new S1(); }
        class S2 : IS2 { public void Use() { } public static IS2 Instance => new S2(); }
    }

    [ApiController]
    public class WithServicesInitializedWithMixedStrategies : ControllerBase // Noncompliant
    {
        private readonly IS1 s1;
        private readonly IS2 s2 = S2.Instance;
        private readonly IS3 s3;

        public WithServicesInitializedWithMixedStrategies(IS1 s1, IServiceProvider serviceProvider)
        {
            this.s1 = s1;
            s3 = serviceProvider.GetRequiredService<IS3>();
        }

        public void A1() { s1.Use(); } // Secondary {{May belong to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #2.}}
        public void A3() { s3.Use(); } // Secondary {{May belong to responsibility #3.}}

        class S2 : IS2 { public void Use() { } public static IS2 Instance => new S2(); }
    }

    [ApiController]
    public class WithAWellKnownInterfaceIncluded : ControllerBase // Noncompliant
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

        public void A1() { Logger.Use(); s1.Use(); } // Secondary {{May belong to responsibility #1.}}
        public void A2() { Logger.Use(); s2.Use(); } // Secondary {{May belong to responsibility #2.}}
        public void A3() { Logger.Use(); s3.Use(); } // Secondary {{May belong to responsibility #3.}}

        class S2 : IS2 { public void Use() { } public static IS2 Instance => new S2(); }
    }
}

[ApiController]
public class WithHttpClientFactory : ControllerBase // Noncompliant
{
    private readonly IHttpClientFactory _httpClientFactory; // Well-known
    private readonly IS1 s1;
    private readonly IS2 s2;

    public void A1() { _httpClientFactory.Use(); s1.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A2() { _httpClientFactory.Use(); s2.Use(); } // Secondary {{May belong to responsibility #2.}}
    public void A3() { _httpClientFactory.Use(); }
}

[ApiController]
public class WithUseInComplexBlocks : ControllerBase // Noncompliant
{
    IS1 s1; IS2 s2; IS3 s3; IS4 s4; IS5 s5; IS6 s6; IS7 s7; IS8 s8; IS9 s9; IS10 s10;

    public void If()     // Secondary {{May belong to responsibility #2.}}
    {
        if (true) { s1.Use(); } else { if (false) { s2.Use(); } }
    }

    public void Switch() // Secondary {{May belong to responsibility #2.}}
    {
        switch (0) { case 0: s2.Use(); break; case 1: s3.Use(); break; }
    }

    public void For()    // Secondary {{May belong to responsibility #2.}}
    {
        for (int i = 0; i < 1; i++) { s1.Use(); s3.Use(); }
    }

    public void TryCatchFinally()      // Secondary {{May belong to responsibility #3.}}
    {
        try { s4.Use(); } catch { s5.Use(); } finally { try { s6.Use(); } catch { s4.Use(); } }
    }

    public void Using()                // Secondary {{May belong to responsibility #3.}}
    {
        using (new ADisposable()) { s5.Use(); s7.Use(); }
    }

    public void BlocksAndParentheses() // Secondary {{May belong to responsibility #1.}}
    {
        { { ((s8)).Use(); } }
    }

    public void NestedLocalFunctions() // Secondary {{May belong to responsibility #1.}}
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

[ApiController]
public class WithMethodsDependingOnEachOther : ControllerBase // Noncompliant
{
    IS1 s1; IS2 s2; IS3 s3; IS4 s4; IS5 s5; IS6 s6; IS7 s7;

    // Chain: A2 to A1
    void A1() { s1.Use(); }            // Secondary {{May belong to responsibility #1.}}
    void A2() { s2.Use(); A1(); }      // Secondary {{May belong to responsibility #1.}}
    void A3() { s2.Use(); }            // Secondary {{May belong to responsibility #1.}}
    // 1-cycle A4 => no service used => ignore
    void A4() { A4(); }
    // 2-cycle A5, A6 => no service used => ignore
    void A5() { A6(); }
    void A6() { A5(); }
    // 3-cycle A7, A8, A9 => no service used => ignore
    void A7() { A8(); }
    void A8() { A9(); }
    void A9() { A7(); }
    // 3-cycle A10, A11, A12 with A13 depending on A12 via s3
    void A10() { A11(); }              // Secondary {{May belong to responsibility #2.}}
    void A11() { A12(); }              // Secondary {{May belong to responsibility #2.}}
    void A12() { A10(); s3.Use(); }    // Secondary {{May belong to responsibility #2.}}
    void A13() { s3.Use(); }           // Secondary {{May belong to responsibility #2.}}
    // 3-cycle A14, A15, A16 with chain A18 -> A15 via s4
    void A14() { A15(); s4.Use(); }    // Secondary {{May belong to responsibility #3.}}
    void A15() { A16(); }              // Secondary {{May belong to responsibility #3.}}
    void A16() { A14(); }              // Secondary {{May belong to responsibility #3.}}
    void A17() { s4.Use(); }           // Secondary {{May belong to responsibility #3.}}
    // Independent method => no service used => ignore
    void A18() { }
    // Independent method with its own service
    void A19() { s5.Use(); }           // Secondary {{May belong to responsibility #4.}}
    // Two actions calling a third one
    void A20() { A22(); }              // Secondary {{May belong to responsibility #5.}}
    void A21() { A22(); }              // Secondary {{May belong to responsibility #5.}}
    void A22() { s6.Use(); s7.Use(); } // Secondary {{May belong to responsibility #5.}}
}

[ApiController]
public class WithServiceProvidersInjectionCoupled : ControllerBase // Compliant: s1Provider and s2Provider are known to provide services
{
    private readonly Func<IS1> s1Provider;
    private readonly Func<int, IS2> s2Provider;

    public WithServiceProvidersInjectionCoupled(Func<IS1> s1Provider, Func<int, IS2> s2Provider)
    {
        this.s1Provider = s1Provider;
        this.s2Provider = s2Provider;
    }

    public void A1() { s1Provider().Use(); var s2 = s2Provider(42); s2.Use(); }
    public void A2() { (s2Provider(42)).Use(); }
}

[ApiController]
public class ApiController : ControllerBase { }

public class DoesNotInheritDirectlyFromControllerBase(IS1 s1, IS2 s2) : ApiController // Compliant, we report only in classes that inherit directly from ControllerBase
{
    public IActionResult A1() { s1.Use(); return Ok(); }
    public IActionResult A2() { s2.Use(); return Ok(); }
}

public class InheritsFromController(IS1 s1, IS2 s2) : Controller // Compliant, we report only in classes that inherit directly from ControllerBase
{
    public IActionResult A1() { s1.Use(); return Ok(); }
    public IActionResult A2() { s2.Use(); return Ok(); }
}

public abstract class AbstractController (IS1 s1, IS2 s2) : ControllerBase // Compliant, we don't report on abstract controllers
{
    public IActionResult A1() { s1.Use(); return Ok(); }
    public IActionResult A2() { s2.Use(); return Ok(); }
    public abstract IActionResult A3();
}


[ApiController]
public class WithServiceProvidersInjectionUsedInGroups : ControllerBase // Noncompliant
{
    private IS1 _s1;
    private Func<bool, uint, Func<double>, IS5> _s5Provider;

    private Func<IS2> S2Provider { get; }
    private Func<int, IS3> S3Provider => i => null;
    private Func<string, char, IS4> S4Provider { get; set; } = (s, c) => null;
    private Func<bool, uint, Func<double>, IS5> S5Provider => _s5Provider;

    public void A1() { _s1.Use(); }                                                             // Secondary {{May belong to responsibility #1.}}
    public void A2() { S2Provider().Use(); S3Provider(42).Use(); }                              // Secondary {{May belong to responsibility #3.}}
    public void A3() { S3Provider(42).Use(); }                                                  // Secondary {{May belong to responsibility #3.}}
    public void A4() { S4Provider("42", '4').Use(); _s5Provider(false, 3u, () => 42.0).Use(); } // Secondary {{May belong to responsibility #2.}}
    public void A5() { _s5Provider(true, 3u, () => 42.0).Use(); }                               // Secondary {{May belong to responsibility #2.}}
    public void A6() { S5Provider(true, 3u, () => 42.0).Use(); }                                // Secondary {{May belong to responsibility #2.}}
}

[ApiController]
public class WithServiceWrappersInjection : ControllerBase // Compliant: no way to know whether s2Invoker wraps a service
{
    private readonly Func<IS1> s1Provider;
    private readonly Action s2Invoker;

    public WithServiceWrappersInjection(Func<IS1> s1Provider, Action s2Invoker)
    {
        this.s1Provider = s1Provider;
        this.s2Invoker = s2Invoker;
    }

    public void A1() { s1Provider().Use(); s2Invoker(); }
    public void A2() { s2Invoker(); }
}

[ApiController]
public class WithLazyServicesInjection : ControllerBase // Noncompliant
{
    private readonly Lazy<IS1> s1Lazy;
    private readonly Lazy<IS2> s2Lazy;
    private readonly Lazy<IS3> s3Lazy;
    private readonly IS4 s4;

    public void A1() { s1Lazy.Value.Use(); s2Lazy.Value.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A2() { s2Lazy.Value.Use(); s4.Use(); }           // Secondary {{May belong to responsibility #1.}}
    public void A3() { s3Lazy.Value.Use(); }                     // Secondary {{May belong to responsibility #2.}}
}

[ApiController]
public class WithMixOfLazyServicesAndServiceProviders : ControllerBase // Noncompliant
{
    private readonly Lazy<IS1> s1Lazy;
    private readonly Lazy<IS2> s2Lazy;
    private readonly IS3 s3;
    private readonly IS4 s4;
    private readonly Func<IS5> s5Provider;
    private readonly Func<IS6> s6Provider;

    public void A1() { s1Lazy.Value.Use(); s3.Use(); }                                                  // Secondary {{May belong to responsibility #1.}}
    public void A2() { s1Lazy.Value.Use(); s2Lazy.Value.Use(); }                                        // Secondary {{May belong to responsibility #1.}}
    public void A3() { s4.Use(); var s5 = s5Provider(); s5.Use(); }                                     // Secondary {{May belong to responsibility #2.}}
    public void A4() { s5Provider().Use(); var s6ProviderAlias = s6Provider(); s6ProviderAlias.Use(); } // Secondary {{May belong to responsibility #2.}}
}

[ApiController]
public class WithIApi : ControllerBase // Compliant
{
    private readonly IApi _api;

    public WithIApi(IApi api) { _api = api; }

    public void A1() { _api.Use(); }
    public void A2() { _api.Use(); }

    public interface IApi : IServiceWithAnAPI { }
}

[ApiController]
public class WithUnusedServices : ControllerBase // Compliant
{
    private readonly IS1 s1;
    private readonly IS2 s2;
    private readonly IS3 s3;
    private readonly IS4 s4;

    public void A1() { }
    public void A2() { }
    public void A3() { }
    public void A4() { }
}

[ApiController]
public class WithUnusedAndUsedServicesFormingTwoGroups : ControllerBase // Compliant
{
    // s2, s3 and s4 form a non-singleton, but no action use any of them => the set is filtered out
    private readonly IS1 s1;
    private readonly IS2 s2;
    private readonly IS3 s3;
    private readonly IS4 s4;

    public void A1() { s1.Use(); }
}

[ApiController]
public class WithUnusedAndUsedServicesFormingThreeGroups : ControllerBase // Noncompliant {{This controller has multiple responsibilities and could be split into 3 smaller controllers.}}
{
    private readonly IS1 s1;
    private readonly IS2 s2;
    private readonly IS3 s3;
    private readonly IS4 s4;

    public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A3() { s3.Use(); } // Secondary {{May belong to responsibility #2.}}
    public void A4() { s4.Use(); } // Secondary {{May belong to responsibility #3.}}
}

[ApiController]
public class WithMethodOverloads : ControllerBase // Noncompliant
{
    private readonly IS1 s1;
    private readonly IS2 s2;
    private readonly IS3 s3;
    private readonly IS4 s4;
    private readonly IS5 s5;
    private readonly IS6 s6;

    public void A1() { s1.Use(); }                // Secondary {{May belong to responsibility #1.}}
    public void A1(int i) { s2.Use(); }           // Secondary {{May belong to responsibility #1.}}
    public void A1(string s) { s3.Use(); }        // Secondary {{May belong to responsibility #1.}}
    public void A1(int i, string s) { s4.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A2() { s5.Use(); }                // Secondary {{May belong to responsibility #2.}}
    public void A2(int i) { s6.Use(); }           // Secondary {{May belong to responsibility #2.}}
}

[ApiController]
public class WithIndexer : ControllerBase // Noncompliant
{
    private readonly IS1 s1;
    private readonly IS2 s2;
    private readonly IS3 s3;

    public int this[int i] { get { s1.Use(); return 42; } set { s2.Use(); } } // Clamp A1 and A2 together

    public void A1() { s1.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A3() { s3.Use(); } // Secondary {{May belong to responsibility #2.}}
}

[ApiController]
public class WithIndexerOverloads : ControllerBase // Noncompliant
{
    private readonly IS1 s1;
    private readonly IS2 s2;
    private readonly IS3 s3;
    private readonly IS4 s4;
    private readonly IS5 s5;

    public int this[int i] { get { s1.Use(); return 42; } set { s2.Use(); } }  // Clamp A1 and A2 together
    public int this[long i] { get { s3.Use(); return 42; } set { s4.Use(); } } // Clamp A1 and A2 together

    public void A1() { s1.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A3() { s3.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A4() { s4.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A5() { s5.Use(); } // Secondary {{May belong to responsibility #2.}}
}

[ApiController]
public class WithIndexerArrow : ControllerBase // Noncompliant
{
    private readonly IS1ReturningInt s1;
    private readonly IS2ReturningInt s2;
    private readonly IS3ReturningInt s3;

    public int this[int i] => 42 + s1.GetValue() + s2.GetValue(); // Clamp A1 and A2 together

    public void A1() { s1.GetValue(); } // Secondary {{May belong to responsibility #1.}}
    public void A2() { s2.GetValue(); } // Secondary {{May belong to responsibility #1.}}
    public void A3() { s3.GetValue(); } // Secondary {{May belong to responsibility #2.}}

    public interface IS1ReturningInt { int GetValue(); }
    public interface IS2ReturningInt { int GetValue(); }
    public interface IS3ReturningInt { int GetValue(); }
}

[ApiController]
public class WithClassInjection : ControllerBase // Noncompliant
{
    private readonly Class1 s1;
    private readonly Class2 s2;
    private readonly Class3 s3;

    public void A1() { s1.Use(); }            // Secondary {{May belong to responsibility #1.}}
    public void A2() { s2.Use(); }            // Secondary {{May belong to responsibility #2.}}
    public void A3() { s3.Use(); }            // Secondary {{May belong to responsibility #1.}}
    public void A4() { s3.Use(); s1.Use(); }  // Secondary {{May belong to responsibility #1.}}
    public void A5() { }                      // No service used => ignore
}

[ApiController]
public class WithStructInjection : ControllerBase // Noncompliant
{
    private readonly Struct1 s1;
    private readonly Struct2 s2;
    private readonly Struct3 s3;

    public void A1() { s1.Use(); }            // Secondary {{May belong to responsibility #1.}}
    public void A2() { s2.Use(); }            // Secondary {{May belong to responsibility #2.}}
    public void A3() { s3.Use(); }            // Secondary {{May belong to responsibility #1.}}
    public void A4() { s3.Use(); s1.Use(); }  // Secondary {{May belong to responsibility #1.}}
    public void A5() { }                      // No service used => ignore
}

[ApiController]
public class WithMixOfInjectionTypes : ControllerBase // Noncompliant
{
    private readonly IS1 interface1;
    private readonly IS2 interface2;
    private readonly Class1 class1;
    private readonly Class2 class2;
    private readonly Class3 class3;   // Unused
    private readonly Struct1 struct1;
    private readonly Struct2 struct2;
    private readonly Struct3 struct3; // Unused

    private readonly Lazy<IS1> lazy1;
    private readonly Lazy<Class1> lazy2;
    private readonly Lazy<Class2> lazy3;
    private readonly Lazy<Struct1> lazy4;
    private readonly Lazy<Struct2> lazy5;
    private readonly Lazy<Lazy<IS1>> lazy6;
    private readonly Lazy<Lazy<IS2>> lazy7; // Unused

    private readonly Func<IS1> delegate1;
    private readonly Func<Class1> delegate2;
    private readonly Func<Struct1> delegate3;
    private readonly Func<Func<IS1>> delegate4;
    private readonly Func<Func<IS2>> delegate5; // Unused

    public void A1() { interface1.Use(); class1.Use(); }                // Secondary {{May belong to responsibility #1.}}
    public void A2() { interface2.Use(); class1.Use(); struct2.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A3() { class2.Use(); }                                  // Secondary {{May belong to responsibility #4.}}
    public void A4() { struct1.Use(); class2.Use(); _ = lazy4; }        // Secondary {{May belong to responsibility #4.}}
    public void A5() { struct2.Use(); lazy1.Value.Use(); }              // Secondary {{May belong to responsibility #1.}}
    public void A6() { _ = lazy1.Value; lazy3.Value.Use(); }            // Secondary {{May belong to responsibility #1.}}
    public void A7() { _ = lazy3.ToString(); }                          // Secondary {{May belong to responsibility #1.}}
    public void A8() { lazy4.Value.Use(); _ = lazy5.IsValueCreated; }   // Secondary {{May belong to responsibility #4.}}
    public void A9() { _ = lazy6.GetHashCode(); delegate1().Use(); }    // Secondary {{May belong to responsibility #2.}}
    public void A10() { _ = delegate2(); delegate3.Invoke().Use(); }    // Secondary {{May belong to responsibility #2.}}
    public void A11() { _ = delegate1.Target; ((delegate3())).Use(); }  // Secondary {{May belong to responsibility #2.}}
    public void A12() { _ = delegate4(); }                              // Secondary {{May belong to responsibility #3.}}
    public void A13() { }                                               // No service used => ignored
}

[ApiController]
public class WithDestructor : ControllerBase // Noncompliant
{
    private readonly IS1 s1;
    private readonly IS2 s2;

    ~WithDestructor() { s1.Use(); s2.Use(); }

    public void A1() { s1.Use(); } // Secondary {{May belong to responsibility #1.}}
    public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #2.}}
}

public class NotAControllerForCoverage
{
    private readonly IS1 s1;
    private readonly IS2 s2;
}

namespace CSharp13
{
    //https://sonarsource.atlassian.net/browse/NET-509
    [ApiController]
    public partial class WithFieldBackedPartialProperties : ControllerBase // Noncompliant
    {
        private IS1 _s1;
        private IS2 _s2;

        public partial IS1 S1 { get => _s1; }
        public partial IS2 S2 { get => _s2; init => _s2 = value; }

        public WithFieldBackedPartialProperties(IS1 s1, IS2 s2) { _s1 = s1; _s2 = s2; }

        public void A1() { S1.Use(); } // Secondary {{May belong to responsibility #1.}}
                                       // Secondary @-1 {{May belong to responsibility #1.}}
        public void A2() { S2.Use(); } // Secondary {{May belong to responsibility #2.}}
                                       // Secondary @-1 {{May belong to responsibility #2.}}
    }

    [ApiController]
    public partial class WithPartialIndexer : ControllerBase // Noncompliant
    {
        private readonly IS1 s1;
        private readonly IS2 s2;
        private readonly IS3 s3;

        public partial int this[int i] { get { s1.Use(); return 42; } set { s2.Use(); } } // Clamp A1 and A2 together

        public void A1() { s1.Use(); } // Secondary {{May belong to responsibility #1.}}
                                       // Secondary @-1 {{May belong to responsibility #1.}}
        public void A2() { s2.Use(); } // Secondary {{May belong to responsibility #1.}}
                                       // Secondary @-1 {{May belong to responsibility #1.}}
        public void A3() { s3.Use(); } // Secondary {{May belong to responsibility #2.}}
                                       // Secondary @-1 {{May belong to responsibility #2.}}
    }
}
