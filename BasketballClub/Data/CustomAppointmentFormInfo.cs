using DevExpress.Blazor;

namespace BasketballClub.Data
{
	public class CustomAppointmentFormInfo : SchedulerAppointmentFormInfo
	{
		public CustomAppointmentFormInfo(DxSchedulerAppointmentItem AppointmentItem,
				DxSchedulerDataStorage DataStorage) : base(AppointmentItem, DataStorage) { }

		public string Host
		{
			get { return CustomFields["Host"].ToString(); }
			set { CustomFields["Host"] = value; }
		}
		public string Id
		{
			get { return CustomFields["Id"].ToString(); }
			set { CustomFields["Id"] = value; }
		}
	}
}
