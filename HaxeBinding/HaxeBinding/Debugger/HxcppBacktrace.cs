using System;
using Mono.Debugging.Backend;
using Mono.Debugging.Client;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace MonoDevelop.HaxeBinding
{
	public class HxcppBacktrace : IBacktrace, IObjectValueSource
	{
		int fcount;
		HxcppDbgSession session;

		long threadId;
		object syncLock = new object();

		public HxcppBacktrace (HxcppDbgSession session, int fcount, long threadId)
		{
			this.session = session;
			this.fcount = fcount;
			this.threadId = threadId;
		}

		#region IObjectValueSource implementation

		public ObjectValue[] GetChildren (ObjectPath path, int index, int count, EvaluationOptions options)
		{
			throw new NotImplementedException ();
		}

		public EvaluationResult SetValue (ObjectPath path, string value, EvaluationOptions options)
		{
			throw new NotImplementedException ();
		}

		public ObjectValue GetValue (ObjectPath path, EvaluationOptions options)
		{
			throw new NotImplementedException ();
		}

		public object GetRawValue (ObjectPath path, EvaluationOptions options)
		{
			throw new NotImplementedException ();
		}

		public void SetRawValue (ObjectPath path, object value, EvaluationOptions options)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IBacktrace implementation

		public StackFrame[] GetStackFrames (int firstIndex, int lastIndex)
		{
			List<StackFrame> frames = new List<StackFrame>();
			session.RunCommand (true, "where", new string[0]);
			HxcppStackInfo[] stackElements = new HxcppStackInfo[session.lastResult.stackElements.Count];
			session.lastResult.stackElements.CopyTo (stackElements);
			session.lastResult.stackElements.Clear ();
			foreach (HxcppStackInfo element in stackElements) {
				frames.Add (new StackFrame (0,
				                            new SourceLocation (element.name,
				                                              PathHelper.GetFullPath (session.BaseDirectory, element.file),
				                                              element.line), 
				                            "Haxe"));
			}
			return frames.ToArray();
		}

		public ObjectValue[] GetLocalVariables (int frameIndex, EvaluationOptions options)
		{
			throw new NotImplementedException ();
		}

		public ObjectValue[] GetParameters (int frameIndex, EvaluationOptions options)
		{
			List<ObjectValue> objects = new List<ObjectValue> ();
			return objects.ToArray ();
		}

		public ObjectValue GetThisReference (int frameIndex, EvaluationOptions options)
		{
			throw new NotImplementedException ();
		}

		public ExceptionInfo GetException (int frameIndex, EvaluationOptions options)
		{
			throw new NotImplementedException ();
		}

		public ObjectValue[] GetAllLocals (int frameIndex, EvaluationOptions options)
		{
			session.RunCommand (true, "vars");
			List<ObjectValue> locals = new List<ObjectValue> ();
			lock (syncLock) {
				foreach (string varName in session.lastResult.vars) {
					ObjectValue val;
					ObjectValueFlags flags = ObjectValueFlags.Variable;
					val = ObjectValue.CreatePrimitive (this, new ObjectPath (varName), "dummyInt", new EvaluationResult ("test_val"), flags);
					val.Name = varName;
					locals.Add (val);
				}
			}

			return locals.ToArray ();
		}

		public ObjectValue[] GetExpressionValues (int frameIndex, string[] expressions, EvaluationOptions options)
		{
			session.RunCommand (true, "vars");
			List<ObjectValue> locals = new List<ObjectValue> ();
			lock (syncLock) {
				foreach (string varName in session.lastResult.vars) {
					if (expressions.Contains (varName)) {
						ObjectValue val;
						ObjectValueFlags flags = ObjectValueFlags.Variable;
						val = ObjectValue.CreatePrimitive (this, new ObjectPath (varName), "dummyInt", new EvaluationResult ("test_val"), flags);
						val.Name = varName;
						locals.Add (val);
					}
				}
			}

			return locals.ToArray ();
		}

		public CompletionData GetExpressionCompletionData (int frameIndex, string exp)
		{
			throw new NotImplementedException ();
		}

		public AssemblyLine[] Disassemble (int frameIndex, int firstLine, int count)
		{
			throw new NotImplementedException ();
		}

		public ValidationResult ValidateExpression (int frameIndex, string expression, EvaluationOptions options)
		{
			throw new NotImplementedException ();
		}

		public int FrameCount {
			get {
				return fcount;
			}
		}

		#endregion
	}
}

