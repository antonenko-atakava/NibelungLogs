"use client";

import { getClassColor } from "@/utils/wow/classColors";
import { cn } from "@/lib/utils";

interface ClassBadgeProps {
  className: string | null | undefined;
  children?: React.ReactNode;
  variant?: "default" | "outline" | "text";
}

export function ClassBadge({ 
  className: classNameName, 
  children, 
  variant = "default"
}: ClassBadgeProps) {
  const color = getClassColor(classNameName);

  if (variant === "text") {
    return (
      <span
        className="font-semibold"
        style={{ color }}
      >
        {children || classNameName || "-"}
      </span>
    );
  }

  if (variant === "outline") {
    return (
      <span
        className={cn(
          "inline-flex items-center px-2.5 py-1 rounded-md text-xs font-medium border"
        )}
        style={{
          color,
          borderColor: `${color}40`,
          backgroundColor: `${color}10`,
        }}
      >
        {children || classNameName || "-"}
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
      {children || classNameName || "-"}
    </span>
  );
}
