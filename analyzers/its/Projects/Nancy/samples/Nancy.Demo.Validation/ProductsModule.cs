﻿namespace Nancy.Demo.Validation
{
    using Nancy.Demo.Validation.Models;
    using Nancy.ModelBinding;
    using Nancy.Validation;

    public class ProductsModule : NancyModule
    {
        public ProductsModule() : base("/products")
        {
            Get("/", args =>
            {
                return "Products module";
            });

            Get("/poke", args =>
            {
                var validator =
                    this.ValidatorLocator.GetValidatorForType(typeof(Product));

                return this.Response.AsJson(validator.Description);
            });

            Post<dynamic>("/", args =>
            {
                Product model = this.Bind();
                var result = this.Validate(model);

                if (!result.IsValid)
                {
                    return View["CustomerError", result];
                }

                return 200;
            });
        }
    }
}