import Link from 'next/link';

interface PrimaryButtonProps {
  href?: string;
  onClick?: () => void;
  children: React.ReactNode;
  className?: string;
  'aria-label'?: string;
}

export default function PrimaryButton({ href, onClick, children, className = '', 'aria-label': ariaLabel }: PrimaryButtonProps) {
  const baseClasses = 'inline-flex items-center justify-center px-6 py-3 bg-[var(--primary)] text-[var(--primary-text)] rounded-lg font-semibold hover:bg-[var(--primary-hover)] transition-all duration-200 shadow-md hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--background)] active:scale-[0.98]';
  
  if (href)
    return (
      <Link 
        href={href} 
        className={`${baseClasses} ${className}`}
        aria-label={ariaLabel}
      >
        {children}
      </Link>
    );

  return (
    <button 
      onClick={onClick} 
      className={`${baseClasses} ${className}`}
      aria-label={ariaLabel}
    >
      {children}
    </button>
  );
}

