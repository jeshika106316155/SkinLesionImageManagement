namespace SLI_UploadPicture.Models
{
    public class AnnotationInformation
    {
        public string encounterID { get; set; }
        public string patientID { get; set; }
        public string performerID { get; set; }
        public string svgInput { get; set; }
        public string imageURL { get; set; }
        public string rowslength { get; set; }
    }
}
