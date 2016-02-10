using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;

namespace CodePlex.SharePointInstaller
{
    class CompatibilityDeployer
    {
        public static void Deploy(SPSolution solution, Collection<SPWebApplication> applications, int minCompat, int maxCompat, ILog log)
        {
#if SP2013
            SPSolutionLanguagePack languagePack = solution.GetLanguagePack(0);
            SPCompatibilityRange compatibilityRange = new SPCompatibilityRange(minCompat, maxCompat);
            Type deployType = languagePack.GetType();
            Type[] argumentTypes = new Type[] { typeof(DateTime), applications.GetType(), typeof(SPSolutionDeploymentJobType), typeof(bool), typeof(bool), typeof(bool), compatibilityRange.GetType() };
            ParameterModifier[] modifiers = new ParameterModifier[] { new ParameterModifier(7) };
            MethodInfo deployMethod = deployType.GetMethod("CreateSolutionDeployTimerJob", BindingFlags.Instance | BindingFlags.NonPublic, null, argumentTypes, modifiers);
            DateTime jobTime = GetImmediateJobTime();
            object[] args = new object[] { jobTime, applications, SPSolutionDeploymentJobType.Deploy, true, true, false, compatibilityRange };
            deployMethod.Invoke(languagePack, args);
#endif
        }
        protected static DateTime GetImmediateJobTime()
        {
            return DateTime.Now - TimeSpan.FromDays(1);
        }
    }
}
