// Iconos SVG inline (sin librerías externas). aria-hidden por ser decorativos.
import type { SVGProps } from 'react';

const base = (props: SVGProps<SVGSVGElement>) => ({
  width: 16,
  height: 16,
  viewBox: '0 0 24 24',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 2,
  strokeLinecap: 'round' as const,
  strokeLinejoin: 'round' as const,
  'aria-hidden': true,
  ...props,
});

export const PlusIcon = (props: SVGProps<SVGSVGElement>) => (
  <svg {...base(props)}><path d="M12 5v14M5 12h14" /></svg>
);

export const SearchIcon = (props: SVGProps<SVGSVGElement>) => (
  <svg {...base(props)}><circle cx="11" cy="11" r="7" /><path d="m21 21-4.3-4.3" /></svg>
);

export const EditIcon = (props: SVGProps<SVGSVGElement>) => (
  <svg {...base(props)}>
    <path d="M12 20h9" />
    <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z" />
  </svg>
);

export const DeleteIcon = (props: SVGProps<SVGSVGElement>) => (
  <svg {...base(props)}><path d="M18 6 6 18M6 6l12 12" /></svg>
);
