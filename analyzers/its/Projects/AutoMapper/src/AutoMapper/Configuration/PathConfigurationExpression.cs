﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
namespace AutoMapper.Configuration
{
    using Execution;
    /// <summary>
    /// Member configuration options
    /// </summary>
    /// <typeparam name="TSource">Source type for this member</typeparam>
    /// <typeparam name="TDestination">Destination type for this map</typeparam>
    /// <typeparam name="TMember">Type for this member</typeparam>
    public interface IPathConfigurationExpression<TSource, TDestination, TMember>
    {
        /// <summary>
        /// Specify the source member to map from. Can only reference a member on the <typeparamref name="TSource"/> type
        /// Any null reference exceptions in this expression will be ignored (similar to flattening behavior)
        /// </summary>
        /// <typeparam name="TSourceMember">Member type of the source member to use</typeparam>
        /// <param name="sourceMember">Expression referencing the source member to map against</param>
        void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember);
        /// <summary>
        /// Ignore this member for configuration validation and skip during mapping
        /// </summary>
        void Ignore();
        void Condition(Func<ConditionParameters<TSource, TDestination, TMember>, bool> condition);
    }
    public readonly struct ConditionParameters<TSource, TDestination, TMember>
    {
        public readonly TSource Source { get; }
        public readonly TDestination Destination { get; }
        public readonly TMember SourceMember { get; }
        public readonly TMember DestinationMember { get; }
        public readonly ResolutionContext Context { get; }
        public ConditionParameters(TSource source, TDestination destination, TMember sourceMember, TMember destinationMember, ResolutionContext context)
        {
            Source = source;
            Destination = destination;
            SourceMember = sourceMember;
            DestinationMember = destinationMember;
            Context = context;
        }
    }
    public class PathConfigurationExpression<TSource, TDestination, TMember> : IPathConfigurationExpression<TSource, TDestination, TMember>, IPropertyMapConfiguration
    {
        private readonly LambdaExpression _destinationExpression;
        private LambdaExpression _sourceExpression;
        protected List<Action<PathMap>> PathMapActions { get; } = new List<Action<PathMap>>();
        public PathConfigurationExpression(LambdaExpression destinationExpression, Stack<Member> chain)
        {
            _destinationExpression = destinationExpression;
            MemberPath = new MemberPath(chain);
        }
        public MemberPath MemberPath { get; }
        public MemberInfo DestinationMember => MemberPath.Last;
        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceExpression) => MapFromUntyped(sourceExpression);
        public void Ignore() => PathMapActions.Add(pm => pm.Ignored = true);
        public void MapFromUntyped(LambdaExpression sourceExpression)
        {
            _sourceExpression = sourceExpression ?? throw new ArgumentNullException(nameof(sourceExpression), $"{nameof(sourceExpression)} may not be null when mapping {DestinationMember.Name} from {typeof(TSource)} to {typeof(TDestination)}.");
            PathMapActions.Add(pm =>
            {
                pm.CustomMapExpression = sourceExpression;
                pm.Ignored = false;
            });
        }
        public void Configure(TypeMap typeMap)
        {
            var pathMap = typeMap.FindOrCreatePathMapFor(_destinationExpression, MemberPath, typeMap);
            Apply(pathMap);
        }
        private void Apply(PathMap pathMap)
        {
            foreach (var action in PathMapActions)
            {
                action(pathMap);
            }
        }
        internal static IPropertyMapConfiguration Create(LambdaExpression destination, LambdaExpression source)
        {
            if (destination == null || !destination.IsMemberPath(out var chain))
            {
                return null;
            }
            var reversed = new PathConfigurationExpression<TSource, TDestination, object>(destination, chain);
            if (reversed.MemberPath.Length == 1)
            {
                var reversedMemberExpression = new MemberConfigurationExpression<TSource, TDestination, object>(reversed.DestinationMember, typeof(TSource));
                reversedMemberExpression.MapFromUntyped(source);
                return reversedMemberExpression;
            }
            reversed.MapFromUntyped(source);
            return reversed;
        }
        public LambdaExpression SourceExpression => _sourceExpression;
        public LambdaExpression GetDestinationExpression() => _destinationExpression;
        public IPropertyMapConfiguration Reverse() => Create(_sourceExpression, _destinationExpression);
        public void Condition(Func<ConditionParameters<TSource, TDestination, TMember>, bool> condition) =>
            PathMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) =>
                        condition(new ConditionParameters<TSource, TDestination, TMember>(src, dest, srcMember, destMember, ctxt));
                pm.Condition = expr;
            });
    }
}