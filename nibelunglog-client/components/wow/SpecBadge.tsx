"use client";

import { getClassColor } from "@/utils/wow/classColors";
import { cn } from "@/lib/utils";

interface SpecBadgeProps {
  specName: string;
  className?: string | null;
  children?: React.ReactNode;
  variant?: "default" | "outline" | "text";
}

export function SpecBadge({ 
  specName, 
  className: classNameName,
  children, 
  variant = "outline"
}: SpecBadgeProps) {
  const color = getClassColor(classNameName);

  if (variant === "text") {
    return (
      <span
        className="font-semibold"
        style={{ color }}
      >
        {children || specName || "-"}
      </span>
    );
  }

  if (variant === "outline") {
    return (
      <span
        className={cn(
          "inline-flex items-center px-2.5 py-1 rounded-md text-xs font-medium border cursor-pointer transition-all"
        )}
        style={{
          color,
          borderColor: `${color}40`,
          backgroundColor: `${color}10`,
        }}
      >
        {children || specName || "-"}
      </span>
    );
  }

  return (
    <span
      className={cn(
        "inline-flex items-center px-2.5 py-1 rounded-md text-xs font-medium border"
      )}
      style={{
        color: "#ffffff",
        borderColor: `${color}60`,
        backgroundColor: `${color}20`,
      }}
    >
      {children || specName || "-"}
    </span>
  );
}
