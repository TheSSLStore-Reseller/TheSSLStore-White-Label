
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace WBSSLStore.Domain
{

    public class ProductDetail : IEntity
	{
		/// <summary>
		/// Gets or sets the <c>ProductDetailID</c> column value.
		/// </summary>
		/// <value>The <c>ProductDetailID</c> column value.</value>
        [Key]
        public int ID
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>ProductID</c> column value.
		/// </summary>
		/// <value>The <c>ProductID</c> column value.</value>
        public int ProductID
        {
            get;
            set;
        }

        public string productcode
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>CertName</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>CertName</c> column value.</value>
        public string CertName
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>URL</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>URL</c> column value.</value>
        public string URL
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>MobileFriendly</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>MobileFriendly</c> column value.</value>
        public bool MobileFriendly
        {
            get;
            set;
        }

		

		/// <summary>
		/// Gets or sets the <c>InstantDelivery</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>InstantDelivery</c> column value.</value>
        public bool InstantDelivery
        {
            get;
            set;
        }

	
		/// <summary>
		/// Gets or sets the <c>DocSigning</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>DocSigning</c> column value.</value>
        public bool DocSigning
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>ScanProduct</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>ScanProduct</c> column value.</value>
        public bool ScanProduct
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>BusinessValid</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>BusinessValid</c> column value.</value>
        public bool BusinessValid
        {
            get;
            set;
        }


		/// <summary>
		/// Gets or sets the <c>SanSupport</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>SanSupport</c> column value.</value>
        public bool SanSupport
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>WildcardSupport</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>WildcardSupport</c> column value.</value>
        public bool WildcardSupport
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>GreenBar</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>GreenBar</c> column value.</value>
        public bool GreenBar
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>IssuanceTime</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>IssuanceTime</c> column value.</value>
        public string IssuanceTime
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>Warranty</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>Warranty</c> column value.</value>
        public string Warranty
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>SiteSeal</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>SiteSeal</c> column value.</value>
		public string SiteSeal
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <c>StarRating</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>StarRating</c> column value.</value>
		public string StarRating
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <c>SealInSearch</c> column value.
		/// </summary>
		/// <value>The <c>SealInSearch</c> column value.</value>
        public bool SealInSearch
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>VulnerabilityAssessment</c> column value.
		/// </summary>
		/// <value>The <c>VulnerabilityAssessment</c> column value.</value>
		public bool VulnerabilityAssessment
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <c>ValidationType</c> column value.
		/// </summary>
		/// <value>The <c>ValidationType</c> column value.</value>
		public string ValidationType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <c>ServerLicense</c> column value.
		/// </summary>
		/// <value>The <c>ServerLicense</c> column value.</value>
        public string ServerLicense
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>ShortDesc</c> column value.
		/// </summary>
		/// <value>The <c>ShortDesc</c> column value.</value>
        [System.Web.Mvc.AllowHtml]
		public string ShortDesc
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <c>LongDesc</c> column value.
		/// This column is nullable.
		/// </summary>
		/// <value>The <c>LongDesc</c> column value.</value>
        [System.Web.Mvc.AllowHtml]
		public string LongDesc
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <c>ProductDatasheetUrl</c> column value.
		/// </summary>
		/// <value>The <c>ProductDatasheetUrl</c> column value.</value>
		public string ProductDatasheetUrl
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <c>VideoUrl</c> column value.
		/// </summary>
		/// <value>The <c>VideoUrl</c> column value.</value>
        public string VideoUrl
        {
            get;
            set;
        }

		/// <summary>
		/// Gets or sets the <c>SimilarProducts</c> column value.
		/// </summary>
		/// <value>The <c>SimilarProducts</c> column value.</value>
		public string SimilarProducts
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <c>SealType</c> column value.
		/// </summary>
		/// <value>The <c>SealType</c> column value.</value>
		public string SealType
		{
			get;
			set;
		}


        /// <summary>
        ///// Converts <see cref="System.Data.DataRow"/> to <see cref="ProductDetailRow"/>.
        ///// </summary>
        ///// <param name="row">The <see cref="System.Data.DataRow"/> object to be mapped.</param>
        ///// <returns>A reference to the <see cref="ProductDetailRow"/> object.</returns>
        //public ProductDetailRow MapRow(DataRow row)
        //{
        //    ProductDetailRow mappedObject = new ProductDetailRow();
        //    DataTable dataTable = row.Table;
        //    DataColumn dataColumn;
        //    // Column "ProductDetailID"
        //    dataColumn = dataTable.Columns["ProductDetailID"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.ProductDetailID = (int)row[dataColumn];
        //    // Column "ProductID"
        //    dataColumn = dataTable.Columns["ProductID"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.ProductID = (int)row[dataColumn];
        //    // Column "CertName"
        //    dataColumn = dataTable.Columns["CertName"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.CertName = (string)row[dataColumn];
        //    // Column "URL"
        //    dataColumn = dataTable.Columns["URL"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.URL = (string)row[dataColumn];
        //    // Column "MobileFriendly"
        //    dataColumn = dataTable.Columns["MobileFriendly"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.MobileFriendly = (bool)row[dataColumn];
        //    // Column "InstantDelivery"
        //    dataColumn = dataTable.Columns["InstantDelivery"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.InstantDelivery = (bool)row[dataColumn];
        //    // Column "DocSigning"
        //    dataColumn = dataTable.Columns["DocSigning"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.DocSigning = (bool)row[dataColumn];
        //    // Column "ScanProduct"
        //    dataColumn = dataTable.Columns["ScanProduct"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.ScanProduct = (bool)row[dataColumn];
        //    // Column "BusinessValid"
        //    dataColumn = dataTable.Columns["BusinessValid"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.BusinessValid = (bool)row[dataColumn];
        //    // Column "SanSupport"
        //    dataColumn = dataTable.Columns["SanSupport"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.SanSupport = (bool)row[dataColumn];
        //    // Column "WildcardSupport"
        //    dataColumn = dataTable.Columns["WildcardSupport"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.WildcardSupport = (bool)row[dataColumn];
        //    // Column "GreenBar"
        //    dataColumn = dataTable.Columns["GreenBar"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.GreenBar = (bool)row[dataColumn];
        //    // Column "IssuanceTime"
        //    dataColumn = dataTable.Columns["IssuanceTime"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.IssuanceTime = (string)row[dataColumn];
        //    // Column "Warranty"
        //    dataColumn = dataTable.Columns["Warranty"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.Warranty = (string)row[dataColumn];
        //    // Column "SiteSeal"
        //    dataColumn = dataTable.Columns["SiteSeal"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.SiteSeal = (string)row[dataColumn];
        //    // Column "StarRating"
        //    dataColumn = dataTable.Columns["StarRating"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.StarRating = (string)row[dataColumn];
        //    // Column "SealInSearch"
        //    dataColumn = dataTable.Columns["SealInSearch"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.SealInSearch = (bool)row[dataColumn];
        //    // Column "VulnerabilityAssessment"
        //    dataColumn = dataTable.Columns["VulnerabilityAssessment"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.VulnerabilityAssessment = (bool)row[dataColumn];
        //    // Column "ValidationType"
        //    dataColumn = dataTable.Columns["ValidationType"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.ValidationType = (string)row[dataColumn];
        //    // Column "ServerLicense"
        //    dataColumn = dataTable.Columns["ServerLicense"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.ServerLicense = (string)row[dataColumn];
        //    // Column "ShortDesc"
        //    dataColumn = dataTable.Columns["ShortDesc"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.ShortDesc = (string)row[dataColumn];
        //    // Column "LongDesc"
        //    dataColumn = dataTable.Columns["LongDesc"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.LongDesc = (string)row[dataColumn];
        //    // Column "ProductDatasheetUrl"
        //    dataColumn = dataTable.Columns["ProductDatasheetUrl"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.ProductDatasheetUrl = (string)row[dataColumn];
        //    // Column "VideoUrl"
        //    dataColumn = dataTable.Columns["VideoUrl"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.VideoUrl = (string)row[dataColumn];
        //    // Column "SimilarProducts"
        //    dataColumn = dataTable.Columns["SimilarProducts"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.SimilarProducts = (string)row[dataColumn];
        //    // Column "SealType"
        //    dataColumn = dataTable.Columns["SealType"];
        //    if (!row.IsNull(dataColumn))
        //        mappedObject.SealType = (string)row[dataColumn];
        //    return mappedObject;
        //}

        ///// <summary>
        ///// Returns the string representation of this instance.
        ///// </summary>
        ///// <returns>The string representation of this instance.</returns>
        //public override string ToString()
        //{
        //    System.Text.StringBuilder dynStr = new System.Text.StringBuilder(GetType().Name);
        //    dynStr.Append(':');
        //    dynStr.Append("  ProductDetailID=");
        //    dynStr.Append(ProductDetailID);
        //    dynStr.Append("  ProductID=");
        //    dynStr.Append(ProductID);
        //    dynStr.Append("  CertName=");
        //    dynStr.Append(CertName);
        //    dynStr.Append("  URL=");
        //    dynStr.Append(URL);
        //    dynStr.Append("  MobileFriendly=");
        //    dynStr.Append(IsMobileFriendlyNull ? (object)"<NULL>" : MobileFriendly);
        //    dynStr.Append("  InstantDelivery=");
        //    dynStr.Append(IsInstantDeliveryNull ? (object)"<NULL>" : InstantDelivery);
        //    dynStr.Append("  DocSigning=");
        //    dynStr.Append(IsDocSigningNull ? (object)"<NULL>" : DocSigning);
        //    dynStr.Append("  ScanProduct=");
        //    dynStr.Append(IsScanProductNull ? (object)"<NULL>" : ScanProduct);
        //    dynStr.Append("  BusinessValid=");
        //    dynStr.Append(IsBusinessValidNull ? (object)"<NULL>" : BusinessValid);
        //    dynStr.Append("  SanSupport=");
        //    dynStr.Append(IsSanSupportNull ? (object)"<NULL>" : SanSupport);
        //    dynStr.Append("  WildcardSupport=");
        //    dynStr.Append(IsWildcardSupportNull ? (object)"<NULL>" : WildcardSupport);
        //    dynStr.Append("  GreenBar=");
        //    dynStr.Append(IsGreenBarNull ? (object)"<NULL>" : GreenBar);
        //    dynStr.Append("  IssuanceTime=");
        //    dynStr.Append(IssuanceTime);
        //    dynStr.Append("  Warranty=");
        //    dynStr.Append(Warranty);
        //    dynStr.Append("  SiteSeal=");
        //    dynStr.Append(SiteSeal);
        //    dynStr.Append("  StarRating=");
        //    dynStr.Append(StarRating);
        //    dynStr.Append("  SealInSearch=");
        //    dynStr.Append(SealInSearch);
        //    dynStr.Append("  VulnerabilityAssessment=");
        //    dynStr.Append(VulnerabilityAssessment);
        //    dynStr.Append("  ValidationType=");
        //    dynStr.Append(ValidationType);
        //    dynStr.Append("  ServerLicense=");
        //    dynStr.Append(ServerLicense);
        //    dynStr.Append("  ShortDesc=");
        //    dynStr.Append(ShortDesc);
        //    dynStr.Append("  LongDesc=");
        //    dynStr.Append(LongDesc);
        //    dynStr.Append("  ProductDatasheetUrl=");
        //    dynStr.Append(ProductDatasheetUrl);
        //    dynStr.Append("  VideoUrl=");
        //    dynStr.Append(VideoUrl);
        //    dynStr.Append("  SimilarProducts=");
        //    dynStr.Append(SimilarProducts);
        //    dynStr.Append("  SealType=");
        //    dynStr.Append(SealType);
        //    return dynStr.ToString();
        //}
	} // End of ProductDetailRow_Base class
} // End of namespace
