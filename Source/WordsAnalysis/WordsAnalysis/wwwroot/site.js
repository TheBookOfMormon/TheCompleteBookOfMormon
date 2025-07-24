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

      if (firstElement instanceof HTMLInputElement || firstElement instanceof HTMLTextAreaElement) {
         firstElement.select();
      }
   }
}

export function scrollBodyToTopLeft() {
   const body = document.getElementById('body');
   if (body) {
      body.scrollTo({ top: 0, left: 0 });
   }
}

export function firstColumnHasError() {
   const rows = getSelectedTableRows();
   if (rows.length === 0) return false;
   return columnHasError(0, rows);
}

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
}

export function scrollToNextError() {
   const rows = getSelectedTableRows();
   if (rows.length === 0) return true;

   const body = getBodyElement();
   if (!body) return false;

   const bodyRect = body.getBoundingClientRect();
   const maxColumns = Math.max(...rows.map(r => r.querySelectorAll('td').length));

   const headerRow = rows[0];
   const th = headerRow.querySelector('th');
   if (!th) return false;

   const thRight = th.getBoundingClientRect().right + 8;

   for (let colIndex = 0; colIndex < maxColumns; colIndex++) {
      if (columnHasError(colIndex, rows)) {
         const td = headerRow.querySelectorAll('td')[colIndex];
         if (!td) continue;

         const tdRect = td.getBoundingClientRect();
         const scrollOffset = tdRect.left - bodyRect.left - th.offsetWidth;

         if (scrollOffset > 1) {
            body.scrollBy({ left: scrollOffset });
            return true;
         }
      }
   }

   return false;
}

export function centerImagePointInParent(imageId, x, y) {
   const image = document.getElementById(imageId);
   if (!image || !image.parentElement) return;

   const container = image.parentElement;

   const scrollLeft = x - container.clientWidth / 2;
   const scrollTop = y - container.clientHeight / 2;

   container.scrollTo({
      left: scrollLeft,
      top: scrollTop,
      behavior: 'auto' // Use 'auto' for instant scroll
   });
}



// === non-exported reusable functions ===

function getSelectedTableRows() {
   const body = getBodyElement();
   if (!body) return [];

   const allRows = Array.from(body.querySelectorAll('tbody tr'));
   const selectedRows = allRows.filter(row => {
      const th = row.querySelector('th');
      if (!th) return false;
      return (th.className || '').includes('--selected');
   });

   // Fallback: if fewer than 2 rows are selected, treat all as selected
   return selectedRows.length >= 2 ? selectedRows : allRows;
}

function columnHasError(colIndex, rows) {
   if (!rows || rows.length < 2) return false;

   const firstRowCells = rows[0].querySelectorAll('td');
   if (colIndex >= firstRowCells.length) return true;

   const expected = firstRowCells[colIndex].getAttribute('data-Text') || null;

   for (let i = 1; i < rows.length; i++) {
      const cells = rows[i].querySelectorAll('td');
      if (colIndex >= cells.length) return true;

      const actual = cells[colIndex].getAttribute('data-Text') || null;
      if (actual !== expected) return true;
   }

   return false;
}

function getBodyElement() {
   return document.getElementById('body');
}

document.addEventListener('mousemove', function (e) {
   mousePosition.x = e.clientX;
   mousePosition.y = e.clientY;
});
