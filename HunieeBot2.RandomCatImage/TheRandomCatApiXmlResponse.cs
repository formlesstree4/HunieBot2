namespace RandomCatImage.GetResponse
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class response
    {

        private responseData dataField;

        /// <remarks/>
        public responseData data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseData
    {

        private responseDataImage[] imagesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("image", IsNullable = false)]
        public responseDataImage[] images
        {
            get
            {
                return this.imagesField;
            }
            set
            {
                this.imagesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataImage
    {

        private string urlField;

        private string idField;

        private string source_urlField;

        /// <remarks/>
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }

        /// <remarks/>
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public string source_url
        {
            get
            {
                return this.source_urlField;
            }
            set
            {
                this.source_urlField = value;
            }
        }
    }
}

namespace RandomCatImage.VoteResponse
{

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class response
    {

        private responseData dataField;

        /// <remarks/>
        public responseData data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseData
    {

        private responseDataVotes votesField;

        /// <remarks/>
        public responseDataVotes votes
        {
            get
            {
                return this.votesField;
            }
            set
            {
                this.votesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataVotes
    {

        private responseDataVotesVote voteField;

        /// <remarks/>
        public responseDataVotesVote vote
        {
            get
            {
                return this.voteField;
            }
            set
            {
                this.voteField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataVotesVote
    {

        private byte scoreField;

        private string image_idField;

        private ulong sub_idField;

        private string actionField;

        /// <remarks/>
        public byte score
        {
            get
            {
                return this.scoreField;
            }
            set
            {
                this.scoreField = value;
            }
        }

        /// <remarks/>
        public string image_id
        {
            get
            {
                return this.image_idField;
            }
            set
            {
                this.image_idField = value;
            }
        }

        /// <remarks/>
        public ulong sub_id
        {
            get
            {
                return this.sub_idField;
            }
            set
            {
                this.sub_idField = value;
            }
        }

        /// <remarks/>
        public string action
        {
            get
            {
                return this.actionField;
            }
            set
            {
                this.actionField = value;
            }
        }
    }


}

namespace RandomCatImage.GetVotesResponse
{

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class response
    {

        private responseData dataField;

        /// <remarks/>
        public responseData data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseData
    {

        private responseDataImage[] imagesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("image", IsNullable = false)]
        public responseDataImage[] images
        {
            get
            {
                return this.imagesField;
            }
            set
            {
                this.imagesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataImage
    {

        private string idField;

        private string urlField;

        private ulong sub_idField;

        private string createdField;

        private byte scoreField;

        /// <remarks/>
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }

        /// <remarks/>
        public ulong sub_id
        {
            get
            {
                return this.sub_idField;
            }
            set
            {
                this.sub_idField = value;
            }
        }

        /// <remarks/>
        public string created
        {
            get
            {
                return this.createdField;
            }
            set
            {
                this.createdField = value;
            }
        }

        /// <remarks/>
        public byte score
        {
            get
            {
                return this.scoreField;
            }
            set
            {
                this.scoreField = value;
            }
        }
    }


}

namespace RandomCatImage.GetFavouritesResponse
{

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class response
    {

        private responseData dataField;

        /// <remarks/>
        public responseData data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseData
    {

        private responseDataImage[] imagesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("image", IsNullable = false)]
        public responseDataImage[] images
        {
            get
            {
                return this.imagesField;
            }
            set
            {
                this.imagesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataImage
    {

        private string idField;

        private string urlField;

        private ulong sub_idField;

        private string createdField;

        /// <remarks/>
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }

        /// <remarks/>
        public ulong sub_id
        {
            get
            {
                return this.sub_idField;
            }
            set
            {
                this.sub_idField = value;
            }
        }

        /// <remarks/>
        public string created
        {
            get
            {
                return this.createdField;
            }
            set
            {
                this.createdField = value;
            }
        }
    }


}

namespace RandomCatImage.CategoriesResponse
{

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class response
    {

        private responseData dataField;

        /// <remarks/>
        public responseData data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseData
    {

        private responseDataCategory[] categoriesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("category", IsNullable = false)]
        public responseDataCategory[] categories
        {
            get
            {
                return this.categoriesField;
            }
            set
            {
                this.categoriesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataCategory
    {

        private byte idField;

        private string nameField;

        /// <remarks/>
        public byte id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }


}

namespace RandomCatImage.StatsResponse
{

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class response
    {

        private responseData dataField;

        /// <remarks/>
        public responseData data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseData
    {

        private responseDataStats statsField;

        /// <remarks/>
        public responseDataStats stats
        {
            get
            {
                return this.statsField;
            }
            set
            {
                this.statsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataStats
    {

        private responseDataStatsStatsoverview statsoverviewField;

        /// <remarks/>
        public responseDataStatsStatsoverview statsoverview
        {
            get
            {
                return this.statsoverviewField;
            }
            set
            {
                this.statsoverviewField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class responseDataStatsStatsoverview
    {

        private byte total_get_requestsField;

        private byte total_votesField;

        private byte total_favouritesField;

        /// <remarks/>
        public byte total_get_requests
        {
            get
            {
                return this.total_get_requestsField;
            }
            set
            {
                this.total_get_requestsField = value;
            }
        }

        /// <remarks/>
        public byte total_votes
        {
            get
            {
                return this.total_votesField;
            }
            set
            {
                this.total_votesField = value;
            }
        }

        /// <remarks/>
        public byte total_favourites
        {
            get
            {
                return this.total_favouritesField;
            }
            set
            {
                this.total_favouritesField = value;
            }
        }
    }


}