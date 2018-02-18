namespace Terrasoft.Core.Process
{
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.Globalization;
	using System.Text;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.Configuration;
	using Terrasoft.Core.DB;
	using Terrasoft.Core.Entities;
	using Terrasoft.Core.Process;
	using Terrasoft.Core.Process.Configuration;
    using System.Net;
    using System.Linq;
	#region Class: TelegramBotMethodsWrapper

	/// <exclude/>
	public class TelegramBotMethodsWrapper : ProcessModel
	{

		public TelegramBotMethodsWrapper(Process process)
			: base(process) {
			AddScriptTaskMethod("ScriptTask1Execute", ScriptTask1Execute);
		}

		#region Methods: Private

		private bool ScriptTask1Execute(ProcessExecutingContext context) {
            var token = "481061368:AAFB-5FPG6jdvYhew2OIOiDIHwPrsHNNa3M";

            int chatId = 330876936;
            string json = GetJSON(token);
            if (IsNewCommand(json))
            {
                var command = GetCommand(json);
                if (command.StartsWith("/newclaim"))
                {
                    AddClaim(context, 4, GetCommand(json).Replace("newclaim ", ""));
                    SendMessage(token, chatId, "Создано обращение <номер созданной заявки>");
                }
                else
                if (command.StartsWith("/claim"))
                {
                    var parsedCommand = GetCommand(json).Split(' ');
                    var claimNum = Convert.ToInt32(parsedCommand[1]);
                    if (command.EndsWith("towork"))
                    {
                        SetClaimStage(context, 4, new Guid("8A29E81A-0B7B-4D13-A701-CB5654A00077"));
                    }
                    else
                    if (command.Contains("ok"))
                    {
                        var claimMessage = parsedCommand[3];
                        SetClaimStage(context, claimNum, new Guid("54614EE2-F51F-42C6-80E9-303F4D29D60C"));
                        Guid claimId_ = GetClaimIdByNumber(context, claimNum);
                        AddHistory(context, claimId_, claimMessage);
                    }
                    else
                    if (command.EndsWith("end"))
                    {
                        SetClaimStage(context, claimNum, new Guid("8A29E81A-0B7B-4D13-A701-CB5654A00077"));
                    }
                    else
                    {
                        var claimMessage = parsedCommand[2];
                        AddClaim(context, claimNum, GetCommand(json).Replace("claim ", ""));
                        Guid claimId = GetClaimIdByNumber(context, claimNum);
                        AddHistory(context, claimId, claimMessage);
                    }
                }
            }
            return true;
		}

        private void AddClaim(ProcessExecutingContext context, int number, string text)
        {
            new Insert(context.UserConnection).Into("Claim").
                                               Set("CategoryId", Column.Parameter(new Guid("20D53E1B-EC9B-4C9A-AA83-3CAC9A674D12"))).
                                               Set("PriorityId", Column.Parameter(new Guid("CBBF4509-5975-46D5-AB71-0C7DC64C8D1C"))).
                                               Set("Text", Column.Parameter(text)).
                                               Set("Number", Column.Parameter(number)).
                                               Execute();
        }

        private Guid GetClaimIdByNumber(ProcessExecutingContext context, int number)
        {
            var esqClaimId = new EntitySchemaQuery(context.UserConnection.EntitySchemaManager, "Claim");
            var idColumn = esqClaimId.AddColumn("Id");
            var filter = esqClaimId.CreateFilterWithParameters(FilterComparisonType.Equal, "Number", number);
            esqClaimId.Filters.Add(filter);
            Guid claimId = esqClaimId.GetEntityCollection(context.UserConnection)[0].GetTypedColumnValue<Guid>(idColumn.Name);
            return claimId;
        }

        private void AddHistory(ProcessExecutingContext context, Guid claimId, string text)
        {
            new Insert(context.UserConnection).Into("SlHistory").
                                               Set("ClaimId", Column.Parameter(claimId)).
                                               Set("MessageText", Column.Parameter(text)).
                                               Execute();
        }

        private void SetClaimStage(ProcessExecutingContext context, int number, Guid stageId)
        {
            var esqClaim = new EntitySchemaQuery(context.UserConnection.EntitySchemaManager, "Claim");
            esqClaim.AddAllSchemaColumns();
            Guid claimId = GetClaimIdByNumber(context, number);
            var claim = esqClaim.GetEntity(context.UserConnection, claimId);
            claim.SetColumnValue("StageId", stageId);
            claim.Save();
        }

        private static bool IsNewCommand(string json)
        {
            var lastCommandSentSecondsNum = (DateTime.Now - GetCommandTime(json)).Seconds;
            if (lastCommandSentSecondsNum < 10)
            {
                return true;
            }
            return false;
        }

        private static string GetJSON(string token)
        {
            string json = new WebClient().DownloadString("https://api.telegram.org/bot" + token + "/getUpdates");
            return json;
        }

        private static string GetCommand(string json)
        {
            string command = Array.FindAll(json.Split(','), s => s.Contains("text")).Last().Split(':').Last().Replace("\"", "");
            return command;
        }

        private static DateTime GetCommandTime(string json)
        {
            string unixDate = Array.FindAll(json.Split(','), s => s.Contains("date")).Last().Split(':').Last().Replace("\"", "");
            var timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(unixDate));
            var localDateTime = new DateTime(timeSpan.Ticks).ToLocalTime();
            return localDateTime;
        }

        private static void SendMessage(string token, int chatId, string text)
        {
            new WebClient().DownloadString("https://api.telegram.org/bot" + token + "/sendMessage?chat_id=" + chatId + "&text=" + text + "");
        }
        #endregion
    }

	#endregion

	#region Class: TelegramBot

	/// <exclude/>
	public class TelegramBot : Terrasoft.Core.Process.Process
	{

		#region Class: ProcessLane1

		/// <exclude/>
		public class ProcessLane1 : ProcessLane
		{

			public ProcessLane1(UserConnection userConnection, TelegramBot process)
				: base(userConnection) {
				Owner = process;
				IsUsedParentUserContexts = false;
			}

		}

		#endregion

		#region Class: IntermediateCatchTimer1FlowElement

		/// <exclude/>
		public class IntermediateCatchTimer1FlowElement : ProcessIntermediateCatchTimerEvent
		{

			#region Constructors: Public

			public IntermediateCatchTimer1FlowElement(UserConnection userConnection, TelegramBot process)
				: base(userConnection) {
				UId = Guid.NewGuid();
				Owner = process;
				Type = "ProcessSchemaIntermediateCatchTimerEvent";
				Name = "IntermediateCatchTimer1";
				IsLogging = true;
				SchemaElementUId = new Guid("0983e9b1-f489-4de1-a87b-ce17ea801893");
				CreatedInSchemaUId = process.InternalSchemaUId;
			}

			#endregion

			#region Properties: Public

			private int _startOffset = 2;
			public override int StartOffset {
				get {
					return _startOffset;
				}
				set {
					_startOffset = value;
				}
			}

			#endregion

		}

		#endregion

		public TelegramBot(UserConnection userConnection)
			: base(userConnection) {
			InitializeMetaPathParameterValues();
			UId = Guid.NewGuid();
			Name = "TelegramBot";
			SchemaUId = new Guid("5b682508-c391-4013-8af8-0a10f5923a73");
			Caption = Schema.Caption;
			SchemaManagerName = "ProcessSchemaManager";
			SerializeToDB = true;
			SerializeToMemory = true;
			IsLogging = true;
			_notificationCaption = () => { return new LocalizableString((Caption)); };
			ProcessModel = new TelegramBotMethodsWrapper(this);
			InitializeFlowElements();
		}

		#region Properties: Private

		private  Guid InternalSchemaUId {
			get {
				return new Guid("5b682508-c391-4013-8af8-0a10f5923a73");
			}
		}

		#endregion

		#region Properties: Public

		private Func<string> _notificationCaption;
		public  virtual string NotificationCaption {
			get {
				return (_notificationCaption ?? (_notificationCaption = () => null)).Invoke();
			}
			set {
				_notificationCaption = () => { return value; };
			}
		}

		private ProcessLane1 _lane1;
		public  ProcessLane1 Lane1 {
			get {
				return _lane1 ?? ((_lane1) = new ProcessLane1(UserConnection, this));
			}
		}

		private ProcessScriptTask _scriptTask1;
		public  ProcessScriptTask ScriptTask1 {
			get {
				return _scriptTask1 ?? (_scriptTask1 = new ProcessScriptTask() {
					UId = Guid.NewGuid(),
					Owner = this,
					Type = "ProcessSchemaScriptTask",
					Name = "ScriptTask1",
					SchemaElementUId = new Guid("34a969e8-6f4f-4312-9abe-82ef47241796"),
					CreatedInSchemaUId = InternalSchemaUId,
					ExecutedEventHandler = OnExecuted,
					IsLogging = true,
					Script = ProcessModel.GetScriptTaskMethod("ScriptTask1Execute"),
				});
			}
		}

		private ProcessFlowElement _startEvent1;
		public  ProcessFlowElement StartEvent1 {
			get {
				return _startEvent1 ?? (_startEvent1 = new ProcessFlowElement() {
					UId = Guid.NewGuid(),
					Owner = this,
					Type = "ProcessSchemaStartEvent",
					Name = "StartEvent1",
					SchemaElementUId = new Guid("b5d7e623-84a5-4b67-86d9-2c358b7fff5f"),
					CreatedInSchemaUId = InternalSchemaUId,
					ExecutedEventHandler = OnExecuted,
					IsLogging = true,
				});
			}
		}

		private IntermediateCatchTimer1FlowElement _intermediateCatchTimer1;
		public  IntermediateCatchTimer1FlowElement IntermediateCatchTimer1 {
			get {
				return _intermediateCatchTimer1 ?? ((_intermediateCatchTimer1) = new IntermediateCatchTimer1FlowElement(UserConnection, this) { ExecutedEventHandler = OnExecuted });
			}
		}

		#endregion

		#region Methods: Private

		private void InitializeFlowElements() {
			FlowElements[ScriptTask1.SchemaElementUId] = new Collection<ProcessFlowElement> { ScriptTask1 };
			FlowElements[StartEvent1.SchemaElementUId] = new Collection<ProcessFlowElement> { StartEvent1 };
			FlowElements[IntermediateCatchTimer1.SchemaElementUId] = new Collection<ProcessFlowElement> { IntermediateCatchTimer1 };
		}

		private void OnExecuted(object sender, ProcessActivityAfterEventArgs e) {
			switch (e.Context.SenderName) {
					case "ScriptTask1":
						e.Context.QueueTasksV2.Enqueue(new ProcessQueueElement("IntermediateCatchTimer1", e.Context.SenderName));
						break;
					case "StartEvent1":
						e.Context.QueueTasksV2.Enqueue(new ProcessQueueElement("ScriptTask1", e.Context.SenderName));
						break;
					case "IntermediateCatchTimer1":
						e.Context.QueueTasksV2.Enqueue(new ProcessQueueElement("ScriptTask1", e.Context.SenderName));
						break;
			}
		}

		private void WritePropertyValues(DataWriter writer, bool useAllValueSources) {
			if (!HasMapping("IntermediateCatchTimer1.StartOffset")) {
				writer.WriteValue("IntermediateCatchTimer1.StartOffset", IntermediateCatchTimer1.StartOffset, 0);
			}
		}

		#endregion

		#region Methods: Protected

		protected override void PrepareStart(ProcessExecutingContext context) {
			base.PrepareStart(context);
			context.Process = this;
			if (IsProcessExecutedBySignal) {
				return;
			}
			context.QueueTasksV2.Enqueue(new ProcessQueueElement("StartEvent1", string.Empty));
		}

		protected override void CompleteApplyingFlowElementsPropertiesData() {
			base.CompleteApplyingFlowElementsPropertiesData();
			foreach (var item in FlowElements) {
				foreach (var itemValue in item.Value) {
					if (Guid.Equals(itemValue.CreatedInSchemaUId, InternalSchemaUId)) {
						itemValue.ExecutedEventHandler = OnExecuted;
					}
				}
			}
		}

		protected override void InitializeMetaPathParameterValues() {
			base.InitializeMetaPathParameterValues();
			MetaPathParameterValues.Add("c1db17d0-c5d1-4068-90c2-a7f5eef93668", () => IntermediateCatchTimer1.StartOffset);
		}

		protected override void ApplyPropertiesDataValues(DataReader reader) {
			base.ApplyPropertiesDataValues(reader);
			bool hasValueToRead = reader.HasValue();
			switch (reader.CurrentName) {
				case "IntermediateCatchTimer1.StartOffset":
					IntermediateCatchTimer1.StartOffset = reader.GetValue<System.Int32>();
				break;
			}
		}

		protected override void WritePropertyValues(DataWriter writer) {
			base.WritePropertyValues(writer);
			WritePropertyValues(writer, true);
		}

		#endregion

		#region Methods: Public

		public override void ThrowEvent(ProcessExecutingContext context, string message) {
			base.ThrowEvent(context, message);
		}

		public override void WritePropertiesData(DataWriter writer, bool writeFlowElements = true) {
			if (Status == Core.Process.ProcessStatus.Inactive) {
				return;
			}
			writer.WriteStartObject(Name);
			base.WritePropertiesData(writer, writeFlowElements);
			WritePropertyValues(writer, false);
			writer.WriteFinishObject();
		}

		public override object CloneShallow() {
			var cloneItem = (TelegramBot)base.CloneShallow();
			cloneItem.ExecutedEventHandler = ExecutedEventHandler;
			return cloneItem;
		}

		#endregion

	}

	#endregion

}