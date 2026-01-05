import Link from 'next/link';

interface SecondaryButtonProps {
  href?: string;
  onClick?: () => void;
  children: React.ReactNode;
  className?: string;
}

export default function SecondaryButton({ href, onClick, children, className = '' }: SecondaryButtonProps) {
  const baseClasses = 'px-8 py-4 bg-transparent border-2 border-white text-white rounded-lg font-semibold hover:bg-white hover:text-black transition-all duration-200';
  
  if (href)
    return (
      <Link href={href} className={`${baseClasses} ${className}`}>
        {children}
      </Link>
    );

  return (
    <button onClick={onClick} className={`${baseClasses} ${className}`}>
      {children}
    </button>
  );
}

