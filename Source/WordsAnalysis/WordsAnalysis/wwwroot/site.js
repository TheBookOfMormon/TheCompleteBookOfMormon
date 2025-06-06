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
      if (firstTd && tableCellHasErrorOrWarning(firstTd)) {
         return true;
      }
   }
   return false;
}

export function scrollToNextWarningOrError() {
   const rows = getSelectedTableRows();
   if (rows.length === 0) return true;

   const body = getBodyElement();
   if (!body) return false;

   const bodyRect = body.getBoundingClientRect();

   for (const row of rows) {
      const th = row.querySelector('th');
      if (!th) continue;

      const thRight = th.getBoundingClientRect().right;

      const tds = Array.from(row.querySelectorAll('td'));
      for (const td of tds) {
         const tdRect = td.getBoundingClientRect();
         if (tdRect.left > thRight && tableCellHasErrorOrWarning(td)) {
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

function tableCellHasErrorOrWarning(td) {
   const cls = td.className || '';
   return cls.includes('--warning')
      || cls.includes('--error')
      || cls.includes('--outlier')
      || cls.includes('--spacer')
      || cls.includes('--word-added-or-removed');
}

function getBodyElement() {
   return document.getElementById('body');
}
