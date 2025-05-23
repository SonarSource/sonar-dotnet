﻿(function (Method) {
    var app = diagnostics.app;

    Method.Model = Backbone.Model.extend({});

    Method.Collection = Backbone.Collection.extend({
        model: Method.Model,

        initialize: function (opts) {
            this.providerName = opts.providerName;
        },

        url: function () {
            return Nancy.config.basePath + "interactive/providers/" + this.providerName;
        }
    });

    Method.Views.List = Backbone.View.extend({
        el: '#main',

        events: {
            'click #back': 'back'
        },

        initialize: function () {
            this.router = app.router;
            this.template = $("#methodList").html();
            this.providerName = this.model.providerName;
        },

        render: function () {
            var methods = this.model.toJSON();

            var html = Handlebars.compile(this.template)({ collection: methods });

            $(this.el).html(html);

            _.each(methods, this.renderItem, this);
        },

        renderItem: function (model) {
            var itemView = new Method.Views.Item({ model: model, providerName: this.providerName });

            this.$('#root').append(itemView.el);
        },

        back: function () {
            this.router.navigate("", true);
        }
    });

    Method.Views.Item = Backbone.View.extend({
        tagName: 'li',

        events: {
            'focus .method-argument input': 'showTooltip',
            'blur .method-argument input': 'hideTooltip',
            'click input[type=button]': 'executeMethod'
        },

        initialize: function (args) {
            this.providerName = args.providerName;
            this.app = app;
            this.router = app.router;
            this.template = $("#method").html();
            this.render();
        },

        render: function () {
            var html = Handlebars.compile(this.template)({ model: this.model });
            $(this.el).append(html);
        },

        showTooltip: function (e) {
            $(e.currentTarget).parent('.tooltip').addClass("tooltip-showing");
        },

        hideTooltip: function (e) {
            $(e.currentTarget).parent('.tooltip').removeClass("tooltip-showing");
        },

        executeMethod: function () {
            var parameters = this.$("input");

            var executionContext = {};

            executionContext.providerName = this.providerName;
            executionContext.methodName = this.model.methodName;
            executionContext.arguments = [];

            _.each(parameters, function (input) {
                if (input.type !== "submit" && input.type !== "button") {
                    executionContext.arguments.push({ name: input.id, value: this.$(input).val() });
                }
            }, this);

            this.app.trigger("execute", executionContext);
        }
    });
})(diagnostics.module("method"));