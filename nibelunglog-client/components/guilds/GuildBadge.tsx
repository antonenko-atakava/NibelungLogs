"use client";

import { cn } from "@/lib/utils";

interface GuildBadgeProps {
  value: number;
  label: string;
  variant?: "default" | "outline" | "primary" | "success";
  children?: React.ReactNode;
}

const variantStyles = {
  default: {
    color: "text-foreground",
    borderColor: "border-border/40",
    backgroundColor: "bg-secondary/20",
  },
  outline: {
    color: "text-foreground",
    borderColor: "border-primary/30",
    backgroundColor: "bg-primary/5",
  },
  primary: {
    color: "text-primary/80",
    borderColor: "border-primary/30",
    backgroundColor: "bg-primary/8",
  },
  success: {
    color: "text-emerald-600/80 dark:text-emerald-400/80",
    borderColor: "border-emerald-500/30",
    backgroundColor: "bg-emerald-500/8",
  },
};

export function GuildBadge({
  value,
  label,
  variant = "default",
  children,
}: GuildBadgeProps) {
  const styles = variantStyles[variant];

  return (
    <span
      className={cn(
        "inline-flex flex-col items-center justify-center px-2.5 py-1.5 rounded-md text-xs font-medium border",
        styles.color,
        styles.borderColor,
        styles.backgroundColor
      )}
    >
      <span className="font-semibold text-sm leading-none">{children || value}</span>
      <span className="text-[10px] mt-0.5 opacity-60 leading-none">{label}</span>
    </span>
  );
}
