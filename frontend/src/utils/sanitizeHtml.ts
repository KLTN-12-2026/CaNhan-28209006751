import DOMPurify from 'dompurify';

const allowedStyleProperties = new Set([
  'text-align', 'font-weight', 'font-style', 'text-decoration', 'vertical-align',
]);

const config = {
  ALLOWED_TAGS: [
    'p', 'div', 'span', 'br', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
    'ul', 'ol', 'li', 'b', 'strong', 'i', 'em', 'u', 'sub', 'sup',
    'blockquote', 'hr', 'a', 'img', 'figure', 'figcaption',
  ],
  ALLOWED_ATTR: ['href', 'src', 'alt', 'title', 'style', 'class', 'target', 'rel'],
};

export function sanitizeHtml(content?: string | null) {
  const wrapper = document.createElement('div');
  wrapper.innerHTML = DOMPurify.sanitize(content ?? '', config);
  wrapper.querySelectorAll<HTMLElement>('[style]').forEach((element) => {
    for (const property of Array.from(element.style)) {
      if (!allowedStyleProperties.has(property)) element.style.removeProperty(property);
    }
    if (!element.getAttribute('style')?.trim()) element.removeAttribute('style');
  });
  wrapper.querySelectorAll<HTMLElement>('[class]').forEach((element) => {
    if (element.tagName === 'P' && element.classList.contains('admin-image-block'))
      element.className = 'admin-image-block';
    else
      element.removeAttribute('class');
  });
  wrapper.querySelectorAll<HTMLAnchorElement>('a[target="_blank"]').forEach((link) => {
    link.rel = 'noopener noreferrer';
  });
  wrapper.querySelectorAll<HTMLImageElement>('img[src^="mailto:" i]').forEach((image) => image.removeAttribute('src'));
  return wrapper.innerHTML;
}

export function sanitizeLessonHtml(
  content: string,
  resolveAssetUrl: (url: string) => string,
  removeFigcaptions = false,
) {
  const wrapper = document.createElement('div');
  wrapper.innerHTML = sanitizeHtml(content);
  if (removeFigcaptions) wrapper.querySelectorAll('figcaption').forEach((caption) => caption.remove());
  wrapper.querySelectorAll<HTMLElement>('[src]').forEach((element) => {
    const src = element.getAttribute('src');
    if (src) element.setAttribute('src', resolveAssetUrl(src));
  });
  return wrapper.innerHTML;
}
