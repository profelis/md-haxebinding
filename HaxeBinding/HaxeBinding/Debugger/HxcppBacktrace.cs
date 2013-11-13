using System;
using Mono.Debugging.Backend;
using Mono.Debugging.Client;
using System.Collections.Generic;
using System.Threading;

namespace MonoDevelop.HaxeBinding
{
	public class HxcppBacktrace : IBacktrace, IObjectValueSource
	{
		int fcount;
		//StackFrame firstFrame;
		HxcppDbgSession session;
		//DissassemblyBuffer[] disBuffers;
		//int currentFrame = -1;
		long threadId;
		bool firstly = true;

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
			Console.WriteLine (firstIndex + " " + lastIndex);
			List<StackFrame> frames = new List<StackFrame>();
			//TODO: fill it up, now it's just a dummy thing to point to the file
			session.lastResult.stackElements.Clear ();
			if (firstly) {
				session.RunCommand (false, "where", new string[0]);
				firstly = false;
			} else {
				lock (session.backtraceLock) {
					if (!Monitor.Wait (session.backtraceLock, 4000))
						throw new InvalidOperationException ("Command execution timeout.");
				}
				foreach (HxcppStackInfo element in session.lastResult.stackElements) {
					frames.Add (new StackFrame (0,
					                          new SourceLocation (element.name,
					                                             PathHelper.GetFullPath (session.classPathes, element.file),
					                                             element.line), 
					                          "Haxe"));
				}
			}
			//frames.Add (new StackFrame (0, new SourceLocation("new", "E:\\dev\\myown\\just_test\\Just_ololo\\Source\\Just_ololo.hx", 15), "Native"));
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
			throw new NotImplementedException ();
		}

		public ObjectValue[] GetExpressionValues (int frameIndex, string[] expressions, EvaluationOptions options)
		{
			throw new NotImplementedException ();
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

