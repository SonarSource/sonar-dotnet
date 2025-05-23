namespace Nancy.Bootstrapper
{
    using System;

    /// <summary>
    /// Represents an instance to be registered into the container
    /// </summary>
    public class InstanceRegistration : ContainerRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceRegistration"/> class.
        /// </summary>
        /// <param name="registrationType">The registration type.</param>
        /// <param name="implementation">The implementation.</param>
        public InstanceRegistration(Type registrationType, object implementation)
        {
            if (registrationType == null)
            {
                throw new ArgumentNullException("registrationType");
            }

            if (implementation == null)
            {
                throw new ArgumentNullException("implementation");
            }

            this.RegistrationType = registrationType;
            this.Implementation = implementation;
            this.Lifetime = Lifetime.Singleton;

            this.ValidateTypeCompatibility(implementation.GetType());
        }

        /// <summary>
        /// Implementation object instance i.e. instance of MyClassThatImplementsIMyInterface
        /// </summary>
        public object Implementation { get; private set; }
    }
}