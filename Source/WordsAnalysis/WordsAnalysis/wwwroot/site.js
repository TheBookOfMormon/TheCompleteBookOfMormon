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

export function scrollToNextWarningOrError() {
   const body = getBodyElement();
   if (!body) return true;

   const allHeaders = getAllHeaders(body);
   if (allHeaders.length === 0) return true;

   const stickyHeaderWidth = allHeaders[0].getBoundingClientRect().width;
   const bodyRect = body.getBoundingClientRect();
   const bodyVisibleLeft = bodyRect.left + stickyHeaderWidth;

   const leftMostVisibleIndex = findLeftMostVisibleColumn(allHeaders, bodyVisibleLeft, bodyRect.right);
   if (leftMostVisibleIndex === -1) return false;

   for (let i = leftMostVisibleIndex + 1; i < allHeaders.length; i++) {
      const th = allHeaders[i];
      if (hasErrorOrWarning(th.className) || columnContainsMin(body, i - 1)) {
         const scrollOffset = th.getBoundingClientRect().left - bodyRect.left - stickyHeaderWidth;
         body.scrollBy({ left: scrollOffset });
         return true;
      }
   }

   return false;
}


export function firstColumnHasErrorOrWarning() {
   const body = getBodyElement();
   if (!body) return false;

   const allHeaders = getAllHeaders(body);
   if (allHeaders.length < 2) return false;

   return hasErrorOrWarning(allHeaders[1].className) || columnContainsMin(body, 0);
}

// === non-exported reusable functions ===

function columnContainsMin(body, columnIndex) {
   const rows = Array.from(body.querySelectorAll('tr'));
   for (const row of rows) {
      const cells = row.querySelectorAll('td');
      if (cells.length > columnIndex) {
         const cellText = cells[columnIndex].innerText || '';
         if (cellText.includes('{min}')) {
            return true;
         }
      }
   }
   return false;
}


function getBodyElement() {
   return document.getElementById('body');
}

function getAllHeaders(body) {
   return Array.from(body.querySelectorAll('th'));
}

function findLeftMostVisibleColumn(headers, leftEdge, rightEdge) {
   let bestIndex = -1;
   let minDistance = Number.POSITIVE_INFINITY;

   for (let i = 1; i < headers.length; i++) {
      const rect = headers[i].getBoundingClientRect();
      if (rect.right > leftEdge && rect.left < rightEdge) {
         const distance = Math.abs(rect.left - leftEdge);
         if (distance < minDistance) {
            minDistance = distance;
            bestIndex = i;
         }
      }
   }

   return bestIndex;
}

function hasErrorOrWarning(className) {
   return className.includes('--warning')
      || className.includes('--error')
      || className.includes('--word-added-or-removed');
}
