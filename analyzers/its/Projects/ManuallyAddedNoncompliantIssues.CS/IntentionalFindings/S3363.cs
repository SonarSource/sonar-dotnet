/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System;

namespace IntentionalFindings
{
	public class Entity
	{
		public DateTime Id { get; set; }	// Noncompliant (S3363)
	}
}
