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
    public class PagingCollectionViewModel : CollectionView
    {
        private readonly IList searchResults;
        private readonly int resultsPerPage;

        private int currentPage = 1;
        private int recordsCount = 0;

        public PagingCollectionViewModel(IList innerList, int itemsPerPage,int totalCount)
            : base(innerList)
        {
            this.searchResults = innerList;
            this.resultsPerPage = itemsPerPage;
            this.recordsCount = totalCount;
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
                this.OnPropertyChanged(new PropertyChangedEventArgs("IsPreviousNavigationEnabled"));
                this.OnPropertyChanged(new PropertyChangedEventArgs("IsNextNavigationEnabled"));
                this.OnPropertyChanged(new PropertyChangedEventArgs("PagingAllowed"));
            }
        }
        public bool PagingAllowed
        {
            get { return (IsPreviousNavigationEnabled || IsNextNavigationEnabled); }

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
                return (this.recordsCount + this.resultsPerPage - 1)
                    / this.resultsPerPage;
            }
        }

        private int EndIndex
        {
            get
            {
                var end = this.currentPage * this.resultsPerPage - 1;
                return (end > this.recordsCount) ? this.recordsCount : end;
            }
        }

        private int StartIndex
        {
            get { return (this.currentPage - 1) * this.resultsPerPage; }
        }

        public override object GetItemAt(int index)
        {
            if (this.searchResults != null && this.searchResults.Count > index)
            {
                var offset = index % (this.resultsPerPage);
                var safeIndex = (int)Math.Min(this.StartIndex + offset, this.recordsCount - 1);
                return this.searchResults[safeIndex];
            }
            return null;
        }

        public void MoveToNextPage()
        {
            //if (this.currentPage < this.PageCount)
            //{
            //    this.CurrentPage += 1;
            //    OnPropertyChanged(new PropertyChangedEventArgs("IsPreviousNavigationEnabled"));
            //    OnPropertyChanged(new PropertyChangedEventArgs("IsNextNavigationEnabled"));
            //}
            //this.Refresh();
         
                MoveToNext();
        }

        public void MoveToPreviousPage()
        {
            //if (this.currentPage > 1)
            //{
            //    this.CurrentPage -= 1;
            //    OnPropertyChanged(new PropertyChangedEventArgs("IsPreviousNavigationEnabled"));
            //    OnPropertyChanged(new PropertyChangedEventArgs("IsNextNavigationEnabled"));
            //}
            //this.Refresh();
          
                MoveToPrev();
        }

        public Action MoveToNext;
        public Action MoveToPrev;
    }
}
