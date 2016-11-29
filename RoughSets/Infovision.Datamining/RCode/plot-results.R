plotressql <- function(resultdata, fieldname, title = "", user_facet_grid = 1) 
{
    d <- resultdata

    ylimitlow = 0
    ylimithigh = 1

    avgfieldname <- paste(fieldname, "_avg", sep = "")
    minfieldname <- paste(fieldname, "_min", sep = "")
    maxfieldname <- paste(fieldname, "_max", sep = "")

    p <- ggplot(data = d, aes_string(x = "eps", y = avgfieldname, group = "model"))

    if (user_facet_grid == 1)
        p <- p + facet_grid(MODEL ~ PRUNINGTYPE) +
            theme(strip.background = element_blank())

    p <- p +
    expand_limits(y = ylimitlow) +
    expand_limits(y = ylimithigh) +
    geom_ribbon(aes_string(ymin = minfieldname, ymax = maxfieldname), colour = "grey20", alpha = 0.1)
    #scale_x_continuous(labels=divider100())

    if (title != "")
        p <- p + annotate("text", x = Inf, y = Inf, label = title, vjust = 1, hjust = 1)
    #p <- p + ggtitle(title)

    p <- p + geom_line(size = .8)
    #p <- p + geom_point(data=subset(resultdata[resultdata$Epsilon %% 10 == 0,], Method %in% series),
    #                    aes(shape=Method), size=2, fill="white")
    p <- p +
    #labs(x = "Approximation degree", y = "Average accuracy") +
    theme_bw(base_size = 24) +
    theme(panel.grid.major = element_line(size = .5, colour = "grey"),
          axis.line = element_line(size = .7, colour = "black"),
          axis.title.x = element_blank(),
          axis.title.y = element_blank(),
          legend.position = "top",
          legend.key.height = unit(0.8, "cm"),
          legend.key.width = unit(1.5, "cm"),
          text = element_text(size = 24),
          plot.margin = unit(c(1, 1, 1, 1), "mm"),
          panel.margin.x = unit(1.25, "lines"))

    #p <- p + 
    #  theme(strip.text.x = element_text(size = 10, colour = "black"))

    return(p)
}
