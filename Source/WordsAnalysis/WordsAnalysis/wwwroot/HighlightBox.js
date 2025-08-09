export function initialize(element) {
   const originalZIndex = element.style.zIndex;

   element.addEventListener("mouseenter", () => {
      element.style.zIndex = 9999;
   });

   element.addEventListener("mouseleave", () => {
      element.style.zIndex = originalZIndex;
   });
}

export function startInteraction(dotNetRef) {
    function onMouseMove(e) {
        dotNetRef.invokeMethodAsync('OnMouseMove', e.clientX, e.clientY);
    }

    function onMouseUp(e) {
        dotNetRef.invokeMethodAsync('OnMouseUp');
        window.removeEventListener('mousemove', onMouseMove);
        window.removeEventListener('mouseup', onMouseUp);
    }

    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
}
