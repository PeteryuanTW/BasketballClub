namespace BasketballClub.Data
{
    public class DxSchedulerAppointment
    {
        public string Id { get; set; }
        public int AppointmentType { get; set; }
		public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Host { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Label { get; set; }
        public int Status { get; set; }
    }
}
