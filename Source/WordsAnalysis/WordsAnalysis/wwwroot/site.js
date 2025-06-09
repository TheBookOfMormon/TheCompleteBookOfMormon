var mousePosition = { x: 0, y: 0 };
const nullGridLocation = { ColumnIndex: -1, RowIndex: -1 };

export function focusFirstElement(container) {
   if (!container) return;

   const element = container instanceof HTMLElement ? container : document.getElementById(container);
   if (!element) return;

   const focusableElements = element.querySelectorAll(
	  'button:not([disabled]), input:not([readonly]):not([disabled]), select:not([disabled]), textarea:not([readonly]):not([disabled]), [tabindex]:not([tabindex="-1"])'
   );

   if (focusableElements.length > 0) {
	  const firstElement = focusableElements[0];
	  firstElement.focus();

	  // If the first element is a text input, select its content
	  if (firstElement instanceof HTMLInputElement || firstElement instanceof HTMLTextAreaElement) {
		 firstElement.select();
	  }
   }
}

export function scrollBodyToTopLeft() {
   const body = document.getElementById('body');
	  body.scrollTo({
		 top: 0,
		 left: 0
	  });
}

export function firstColumnHasErrorOrWarning() {
   const rows = getSelectedTableRows();
   for (const row of rows) {
      const firstTd = row.querySelector('td');
      if (firstTd && tableCellHasErrorOrWarning(firstTd, rows.length > 1)) {
         return true;
      }
   }
   return false;
}

// Function to call from Blazor (or anywhere) to get the cell under the mouse
export function getWordGridLocation() {
   let el = document.elementFromPoint(mousePosition.x, mousePosition.y);

   while (el && el.nodeType === Node.ELEMENT_NODE && el.tagName !== 'TD') {
      el = el.parentElement;
   }
   if (!el || el.tagName !== 'TD') return nullGridLocation;
   const row = el.getAttribute('data-row');
   const column = el.getAttribute('data-column');
   if (row === null || column === null) return nullGridLocation;
   return {
      ColumnIndex: Number(column),
      RowIndex: Number(row),
   };
};


export function scrollToNextWarningOrError() {
   const rows = getSelectedTableRows();
   if (rows.length === 0) return true;

   const body = getBodyElement();
   if (!body) return false;

   const bodyRect = body.getBoundingClientRect();

   const multipleRowsSelected = rows.length > 1;
   for (const row of rows) {
      const th = row.querySelector('th');
      if (!th) continue;

      const thRight = th.getBoundingClientRect().right;

      const tds = Array.from(row.querySelectorAll('td'));
      for (const td of tds) {
         const tdRect = td.getBoundingClientRect();
         if (tdRect.left > thRight && tableCellHasErrorOrWarning(td, multipleRowsSelected)) {
            const scrollOffset = tdRect.left - bodyRect.left - th.offsetWidth;
            if (scrollOffset > 1) {
               body.scrollBy({ left: scrollOffset });
               return true;
            }
         }
      }
   }

   return false;
}

// === non-exported reusable functions ===

function getSelectedTableRows() {
   const body = getBodyElement();
   if (!body) return [];

   const rows = Array.from(body.querySelectorAll('tbody tr'));
   return rows.filter(row => {
      const th = row.querySelector('th');
      if (!th) return false;

      const cls = th.className || '';
      return cls.includes("--selected");
   });
}

function tableCellHasErrorOrWarning(td, multipleRowsSelected) {
   const cls = td.className || '';
   return cls.includes('--warning')
      || cls.includes('--error')
      || cls.includes('--outlier')
      || (cls.includes('--spacer') && multipleRowsSelected);
}

function getBodyElement() {
   return document.getElementById('body');
}


document.addEventListener('mousemove', function (e) {
   mousePosition.x = e.clientX;
   mousePosition.y = e.clientY;
});

