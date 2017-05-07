options(warn = -1)

require(ggplot2)
require(grid)
require(gridExtra)

library(ggplot2)
library(grid)
library(gridExtra)

plotresult <- function(
    dt,
    xField,
    yField,
    groupVertical = "",
    groupHorizontal = "",
    yMinField = "",
    yMaxField = "",    
    title = "",    
    yLimitLow = 0,
    yLimitHigh = 1) {

    p <- ggplot(data = dt, aes_string(x = xField, y = yField, group = groupVertical))

    if (groupVertical != "") {
        if (groupHorizontal != "") {
            p <- p + facet_grid(as.formula(paste(groupVertical, "~", groupHorizontal, sep = "")), scales = "free") + theme(strip.background = element_blank())
        }
        else {
            p <- p + facet_grid(as.formula(paste("~", groupVertical)), scales = "free") + theme(strip.background = element_blank())
        }
    }
    
    p <- p + expand_limits(y = yLimitLow)
    p <- p + expand_limits(y = yLimitHigh)
    #p <- p + scale_x_continuous(limits = c(0, NA))
    
    if (yMinField != "" && yMaxField != "")
        p <- p + geom_ribbon(aes_string(ymin = yMinField, ymax = yMaxField), colour = "grey20", alpha = 0.1)

    if (title != "")
        #p <- p + annotate("text", x = Inf, y = Inf, label = title, vjust = 1, hjust = 1)
        p <- p + ggtitle(title);
        

    p <- p + geom_line(size = .8)
    p <- p +    
    theme_bw(base_size = 20) +
    theme(panel.grid.major = element_line(size = .5, colour = "grey"),
          axis.line = element_line(size = .7, colour = "black"),
          axis.title.x = element_blank(),
          axis.title.y = element_blank(),
          #legend.position = "top",
          #legend.key.height = unit(0.8, "cm"),
          #legend.key.width = unit(1.5, "cm"),
          text = element_text(size = 20),
          plot.margin = unit(c(1, 1, 1, 1), "mm"),
          panel.spacing.x = unit(1.25, "lines"))
    
    return(p)
}

add_hline_to_plot <- function(p, dt, f, groupby) {
    p <- p + geom_hline(data = dt, aes_string(yintercept = f))
    return(p)
}

plotresultsimple <- function(dt, xField, yField, groupBy, yMinField = "",
    yMaxField = "", title = "", yLimitLow = 0, yLimitHigh = 1) {
    p <- ggplot(data = dt, aes_string(x=xField, y=yField, group=groupBy, color=groupBy))
    p <- p + expand_limits(y=yLimitLow)
    p <- p + expand_limits(y=yLimitHigh)
    #if (yMinField != "" && yMaxField != "")
    #    p <- p + geom_ribbon(aes_string(ymin=yMinField, ymax=yMaxField, fill=groupBy), alpha=0.2)
    if (title != "")        
        p <- p + ggtitle(title);    
    p <- p + geom_line(size=.8)    
    p <- p + theme_bw(base_size=20)
    return(p)
}





