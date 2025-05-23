﻿(function (Provider) {
    var app = diagnostics.app;

    Provider.Model = Backbone.Model.extend({});

    Provider.Collection = Backbone.Collection.extend({
        model: Provider.Model,

        url: function () {
            return Nancy.config.basePath + "interactive/providers/";
        }
    });

    Provider.Views.List = Backbone.View.extend({
        el: '#main',

        initialize: function () {
            this.template = $("#providerList").html();
        },

        render: function () {
            var providers = this.model.toJSON();

            var html = Handlebars.compile(this.template)({ collection: providers });

            $(this.el).html(html);

            _.each(providers, this.renderItem, this);
        },

        renderItem: function (model) {
            var itemView = new Provider.Views.Item({ model: model });

            $(this.el).append(itemView.el);
        }
    });

    Provider.Views.Item = Backbone.View.extend({
        className: "item-list",

        events: {
            'click': 'showMethods'
        },

        initialize: function () {
            this.router = app.router;
            this.template = $("#provider").html();
            this.render();
        },

        render: function () {
            var html = Handlebars.compile(this.template)({ model: this.model });
            $(this.el).append(html);
        },

        showMethods: function () {
            this.router.navigate(this.model.name, true);
        }
    });
})(diagnostics.module("provider"))