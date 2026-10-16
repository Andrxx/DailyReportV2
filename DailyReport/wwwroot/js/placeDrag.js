// Перетаскивание пациента между палатами одного отделения (/places, этап 4, 16.10.2026).
// Слушатели висят на document (делегирование), поэтому перерисовка Blazor их не срывает:
// после LoadCards строки и карточки создаются заново, но атрибуты data-* перечитываются
// в момент события. В .NET уходит только само drop; dragover/dragleave обрабатываются
// целиком в браузере, чтобы не гонять десятки событий в секунду по контуру.

let dotNetRef = null;

const draggingClass = 'place-dragging';
const overClass = 'place-drop-over';
const dragIdKey = 'placeDragId';

const rowOf = (el) => (el && el.closest ? el.closest('[data-patient-id]') : null);
const wardOf = (el) => (el && el.closest ? el.closest('[data-drop-ward]') : null);

function clearVisual() {
    document.querySelectorAll('.' + draggingClass).forEach((el) => el.classList.remove(draggingClass));
    document.querySelectorAll('.' + overClass).forEach((el) => el.classList.remove(overClass));
    delete document.documentElement.dataset[dragIdKey];
}

function onDragStart(event) {
    const row = rowOf(event.target);
    if (!row) return;

    const id = row.getAttribute('data-patient-id');
    if (!id || id === '0') return;

    document.documentElement.dataset[dragIdKey] = id;

    if (event.dataTransfer) {
        event.dataTransfer.setData('text/plain', id);
        event.dataTransfer.effectAllowed = 'move';
    }

    row.classList.add(draggingClass);
}

function onDragOver(event) {
    const ward = wardOf(event.target);
    if (!ward || !document.documentElement.dataset[dragIdKey]) return;

    event.preventDefault();
    if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';

    if (!ward.classList.contains(overClass)) {
        document.querySelectorAll('.' + overClass).forEach((el) => el.classList.remove(overClass));
        ward.classList.add(overClass);
    }
}

function onDragLeave(event) {
    const ward = wardOf(event.target);
    if (!ward) return;
    if (event.relatedTarget && ward.contains(event.relatedTarget)) return;
    ward.classList.remove(overClass);
}

async function onDrop(event) {
    const ward = wardOf(event.target);
    const id = document.documentElement.dataset[dragIdKey]
        || (event.dataTransfer ? event.dataTransfer.getData('text/plain') : '');

    if (!ward || !id) {
        clearVisual();
        return;
    }

    event.preventDefault();
    const target = ward.getAttribute('data-drop-ward');
    clearVisual();

    if (dotNetRef) await dotNetRef.invokeMethodAsync('DropPatient', Number(id), target);
}

export function init(ref) {
    dotNetRef = ref;

    if (window.__placeDragBound) return;
    window.__placeDragBound = true;

    document.addEventListener('dragstart', onDragStart);
    document.addEventListener('dragover', onDragOver);
    document.addEventListener('dragleave', onDragLeave);
    document.addEventListener('drop', onDrop);
    document.addEventListener('dragend', clearVisual);
    window.addEventListener('blur', clearVisual);
}

export function release() {
    dotNetRef = null;
    clearVisual();
}
