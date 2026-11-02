namespace SG.Core
{
    namespace Models
    {
        /// <summary>
        /// Enumeration To Represent Subscribe Athorization Status
        /// </summary>
        public enum AuthStatus
        {
            /// <summary>
            /// Compeletly Authorized
            /// </summary>
            Authorized,
            /// <summary>
            /// Subscribe Time Expired
            /// </summary>
            TimeExpired,
            /// <summary>
            /// Hardware Fingerprint Not Macth With User 
            /// </summary>
            FingerprintNotMatch,
            /// <summary>
            /// Username Or Password Is Wrong
            /// </summary>
            WrongUserPass,
            /// <summary>
            /// User Not Found
            /// </summary>
            UserNotFound,
            /// <summary>
            /// Subscribe File Not Found
            /// </summary>
            FileNotFound
        };
        /// <summary>
        /// SG Slider Slide Object
        /// </summary>
        public class SGSlide
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Subject { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string StartDate { get; set; }
            /// <summary>
            ///
            /// </summary>
            public string EndDate { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Image { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Link { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public bool Expired { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGNews
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Date { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Organization { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Subject { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string User { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class RSSNews
        {
            /// <summary>
            /// 
            /// </summary>
            public string Subject { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Summary { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Link { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGCurrency
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int DateID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int Price { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string StartDate { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string EndDate { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Date { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGEducation
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Chapter { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Subject { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Attachment { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGQ
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int FromID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Subject { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string ImageUrl { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Date { get; set; }
        }
        /// <summary>
        ///
        /// </summary>
        public class SGA
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int FromID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int ToID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Answer { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Date { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGLaw
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Date { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Subject { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Attachment { get; set; }
            /// <summary>
            ///
            /// </summary>
            public string From { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGTariff
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string HS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Reference { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string English { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Measure { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGChapterDescription
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string HS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGLicense
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string HS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Licenses { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGLicenseList
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGPriority
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string HS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Priority { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGCompany
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string HS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Company { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Link { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGExportStandard
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Standard { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGExportPrice
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string HS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Brand { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Unit { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Currency { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Price { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string PriceKind { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Paper { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string PaperDate { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGTSC
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int HS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Price { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Country { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Brand { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Model { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Unit { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string BuyKind { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Reference { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Identifier { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGSpare
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Spare1ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Spare2ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Spare3ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string PartNumber { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string PartName { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Price { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGSpare1
        {
            /// <summary>
            /// 
            /// </summary>
            public int Spare1ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Type { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGSpare2
        {
            /// <summary>
            /// 
            /// </summary>
            public int Spare2ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int Spare1ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Type { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGSpare3
        {
            /// <summary>
            /// 
            /// </summary>
            public int Spare3ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int Spare2ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int Spare1ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Type { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGVehiclePrice
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int HS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Price { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Country { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Brand { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Model { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Unit { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string BuyKind { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Reference { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Identifier { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class Hardware
        {
            /// <summary>
            /// 
            /// </summary>
            public CPU CPU { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public BIOS BIOS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Motherboard Motherboard { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class Motherboard
        {
            /// <summary>
            /// 
            /// </summary>
            public string Manufacturer { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Model { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SerialNumber { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class BIOS
        {
            /// <summary>
            /// 
            /// </summary>
            public string Manufacturer { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string IdentificationCode { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SerialNumber { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class CPU
        {
            /// <summary>
            /// 
            /// </summary>
            public string UniqueId { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string ProcessorId { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Manufacturer { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string MaxClockSpeed { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class User
        {
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Phone { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string UserName { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Password { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Hardware Hardware { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SGSubscribe
        {
            /// <summary>
            /// 
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string UserName { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Password { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Hardware { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string ExpireDate { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class SubscribeFile
        {
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string UserName { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Fingerprint { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public bool Expired { get; set; }
        }

        public class SGRegDate
        {
            public string RegisterDate { get; set; }
            public string ExpireDate { get; set; }
        }
    }
}
