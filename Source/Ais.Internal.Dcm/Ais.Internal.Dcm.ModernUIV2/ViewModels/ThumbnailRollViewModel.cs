using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public class ThumbnailRollViewModel : CollectionView
    {
        private readonly IList currentImages;
        private readonly int resultsPerPage;

        private int currentPage = 1;

        public ThumbnailRollViewModel(IList innerList, int itemsPerPage)
            : base(innerList)
        {
            this.currentImages = innerList;
            this.resultsPerPage = itemsPerPage;
        }

        public override int Count
        {
            get { return this.resultsPerPage; }
        }

        public int CurrentPage
        {
            get { return this.currentPage; }
            set
            {
                this.currentPage = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("CurrentPage"));
            }
        }

        public bool IsPreviousNavigationEnabled
        {
            get { return this.currentPage > 1; }
        }

        public bool IsNextNavigationEnabled
        {
            get { return this.currentPage < this.PageCount; }
        }

        public int ItemsPerPage { get { return this.resultsPerPage; } }

        public int PageCount
        {
            get
            {
                return (this.currentImages.Count + this.resultsPerPage - 1)
                    / this.resultsPerPage;
            }
        }

        private int EndIndex
        {
            get
            {
                var end = this.currentPage * this.resultsPerPage - 1;
                return (end > this.currentImages.Count) ? this.currentImages.Count : end;
            }
        }

        private int StartIndex
        {
            get { return (this.currentPage - 1) * this.resultsPerPage; }
        }

        public override object GetItemAt(int index)
        {
            if (this.currentImages != null && this.currentImages.Count > 0)
            {
                var offset = index % (this.resultsPerPage);
                var safeIndex = (int)Math.Min(this.StartIndex + offset, this.currentImages.Count - 1);
                return this.currentImages[safeIndex];
            }
            return null;
        }

        public void MoveToNextPage()
        {
            if (this.currentPage < this.PageCount)
            {
                this.CurrentPage += 1;
                OnPropertyChanged(new PropertyChangedEventArgs("IsPreviousNavigationEnabled"));
                OnPropertyChanged(new PropertyChangedEventArgs("IsNextNavigationEnabled"));
            }
            this.Refresh();
        }

        public void MoveToPreviousPage()
        {
            if (this.currentPage > 1)
            {
                this.CurrentPage -= 1;
                OnPropertyChanged(new PropertyChangedEventArgs("IsPreviousNavigationEnabled"));
                OnPropertyChanged(new PropertyChangedEventArgs("IsNextNavigationEnabled"));
            }
            this.Refresh();
        }
    }
}
