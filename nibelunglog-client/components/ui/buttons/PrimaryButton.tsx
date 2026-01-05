import Link from 'next/link';

interface PrimaryButtonProps {
  href?: string;
  onClick?: () => void;
  children: React.ReactNode;
  className?: string;
}

export default function PrimaryButton({ href, onClick, children, className = '' }: PrimaryButtonProps) {
  const baseClasses = 'px-8 py-4 bg-white text-black rounded-lg font-semibold hover:bg-gray-100 transition-all duration-200 shadow-lg hover:shadow-xl transform hover:-translate-y-0.5';
  
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

